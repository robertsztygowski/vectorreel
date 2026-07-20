#!/usr/bin/env python3
"""
TwelveLabs API hands-on driver (mdreel competitor deep-dive 004, M4).

Exercises the live v1.3 REST API on the free tier and writes the raw (redacted)
request/response JSON for each endpoint into ../api-captures/.

SECRETS: the API key is read from the environment variable TWELVELABS_API_KEY
(populated from the gitignored .env.local). It is NEVER hardcoded here and NEVER
written into any capture file (it only ever lives in the x-api-key request header,
which we do not serialize). Run:

    # PowerShell, from experiments/004-twelvelabs-deep-dive/
    $env:TWELVELABS_API_KEY = (Get-Content .env.local | ? {$_ -match '^TWELVELABS_API_KEY='}) -replace '^TWELVELABS_API_KEY=',''
    python code-samples/run_pipeline.py

Test media: a 60-second Big Buck Bunny clip (CC-BY, Blender Foundation),
downloaded to tmp/ and never committed.
"""
import os, sys, json, time, mimetypes, urllib.request, urllib.error, uuid

BASE = "https://api.twelvelabs.io/v1.3"
KEY = os.environ.get("TWELVELABS_API_KEY")
if not KEY:
    sys.exit("TWELVELABS_API_KEY not set (source it from .env.local)")

HERE = os.path.dirname(os.path.abspath(__file__))
CAP = os.path.abspath(os.path.join(HERE, "..", "api-captures"))
os.makedirs(CAP, exist_ok=True)
CLIP = os.path.abspath(os.path.join(HERE, "..", "tmp", "bbb-60s.mp4"))


def save(name, obj):
    p = os.path.join(CAP, name)
    with open(p, "w", encoding="utf-8") as f:
        json.dump(obj, f, indent=2, ensure_ascii=False)
    print(f"  -> saved {name}")


def req(method, path, body=None, headers=None, raw=None, content_type=None):
    url = BASE + path
    h = {"x-api-key": KEY}
    if headers:
        h.update(headers)
    data = None
    if body is not None:
        data = json.dumps(body).encode()
        h["Content-Type"] = "application/json"
    elif raw is not None:
        data = raw
        if content_type:
            h["Content-Type"] = content_type
    r = urllib.request.Request(url, data=data, headers=h, method=method)
    t0 = time.time()
    try:
        resp = urllib.request.urlopen(r)
        dt = time.time() - t0
        payload = resp.read().decode()
        status = resp.status
    except urllib.error.HTTPError as e:
        dt = time.time() - t0
        payload = e.read().decode()
        status = e.code
    try:
        parsed = json.loads(payload) if payload else {}
    except json.JSONDecodeError:
        parsed = {"_raw": payload}
    return status, dt, parsed


def capture(name, method, path, req_body, status, dt, resp):
    """Store a redacted request/response pair. Key never included."""
    save(name, {
        "endpoint": f"{method} {path}",
        "request_headers": {"x-api-key": "tlk_***REDACTED***", "Content-Type": "application/json"},
        "request_body": req_body,
        "response_status": status,
        "latency_seconds": round(dt, 3),
        "response_body": resp,
    })


def multipart(fields, file_field, file_path):
    boundary = "----mdreel" + uuid.uuid4().hex
    ctype = mimetypes.guess_type(file_path)[0] or "video/mp4"
    parts = []
    for k, v in fields.items():
        parts.append(f"--{boundary}\r\nContent-Disposition: form-data; name=\"{k}\"\r\n\r\n{v}\r\n".encode())
    with open(file_path, "rb") as f:
        filedata = f.read()
    parts.append(
        f"--{boundary}\r\nContent-Disposition: form-data; name=\"{file_field}\"; "
        f"filename=\"{os.path.basename(file_path)}\"\r\nContent-Type: {ctype}\r\n\r\n".encode()
        + filedata + b"\r\n"
    )
    parts.append(f"--{boundary}--\r\n".encode())
    return b"".join(parts), f"multipart/form-data; boundary={boundary}"


def multipart_fields(fields):
    """multipart/form-data body with only text fields (no file)."""
    boundary = "----mdreel" + uuid.uuid4().hex
    parts = []
    for k, v in fields.items():
        parts.append(f"--{boundary}\r\nContent-Disposition: form-data; name=\"{k}\"\r\n\r\n{v}\r\n".encode())
    parts.append(f"--{boundary}--\r\n".encode())
    return b"".join(parts), f"multipart/form-data; boundary={boundary}"


def main():
    results = {}
    reuse_idx = os.environ.get("TL_INDEX_ID")
    reuse_vid = os.environ.get("TL_VIDEO_ID")

    if reuse_idx and reuse_vid:
        print(f"[reuse] index={reuse_idx} video={reuse_vid} (skipping create/upload/poll)")
        idx, video_id = reuse_idx, reuse_vid
        results["index_id"] = idx
        results["video_id"] = video_id
    else:
        # 1. Create index (Marengo 3.0 for search/embed + Pegasus for analyze)
        print("[1] Create index")
        body = {
            "index_name": f"mdreel-m4-{int(time.time())}",
            "models": [
                {"model_name": "marengo3.0", "model_options": ["visual", "audio"]},
                {"model_name": "pegasus1.2", "model_options": ["visual", "audio"]},
            ],
        }
        st, dt, resp = req("POST", "/indexes", body=body)
        capture("01-create-index.json", "POST", "/indexes", body, st, dt, resp)
        print("   status", st, "in", round(dt, 2), "s")
        idx = resp.get("_id")
        if not idx:
            print("   index creation failed:", resp)
            return
        results["index_id"] = idx

        # 2. Upload + index the clip (classic async task endpoint)
        print("[2] Upload + index clip")
        mp, ctype = multipart({"index_id": idx}, "video_file", CLIP)
        st, dt, resp = req("POST", "/tasks", raw=mp, content_type=ctype)
        capture("02-create-task.json", "POST", "/tasks",
                {"index_id": idx, "video_file": "<bbb-60s.mp4 binary omitted>"}, st, dt, resp)
        print("   status", st, "in", round(dt, 2), "s")
        task_id = resp.get("_id")
        if not task_id:
            print("   task creation failed:", resp)
            return
        results["task_id"] = task_id

        # 3. Poll task until ready
        print("[3] Poll indexing task")
        t_start = time.time()
        last = None
        video_id = None
        for _ in range(120):  # up to ~10 min
            st, dt, resp = req("GET", f"/tasks/{task_id}")
            status = resp.get("status")
            if status != last:
                print("   status:", status, f"(+{round(time.time()-t_start)}s)")
                last = status
            if status in ("ready", "failed"):
                video_id = resp.get("video_id")
                capture("03-task-ready.json", "GET", f"/tasks/{task_id}", None, st, dt, resp)
                break
            time.sleep(5)
        index_seconds = round(time.time() - t_start)
        results["index_latency_seconds"] = index_seconds
        results["video_id"] = video_id
        print(f"   indexing took ~{index_seconds}s; video_id={video_id}")
        if last != "ready":
            print("   indexing did not reach ready; stopping")
            save("00-run-summary.json", results)
            return

    # 4. Search (text query) — requires multipart/form-data
    print("[4] Search")
    sfields = {
        "index_id": idx,
        "query_text": "a bunny in a green meadow",
        "search_options": "visual",
        "group_by": "clip",
        "page_limit": "5",
    }
    mp, ctype = multipart_fields(sfields)
    st, dt, resp = req("POST", "/search", raw=mp, content_type=ctype)
    capture("04-search.json", "POST", "/search", sfields, st, dt, resp)
    print("   status", st, "in", round(dt, 2), "s;",
          len(resp.get("data", [])) if isinstance(resp, dict) else "?", "results")

    # 5. Analyze / generate (Pegasus) — non-streaming for a clean capture
    print("[5] Analyze (summary, stream=false)")
    abody = {
        "video_id": video_id,
        "prompt": "Summarize this video in 3 sentences, then list the main visual chapters with start and end timestamps.",
        "temperature": 0.2,
        "stream": False,
    }
    st, dt, resp = req("POST", "/analyze", body=abody)
    capture("05-analyze.json", "POST", "/analyze", abody, st, dt, resp)
    print("   status", st, "in", round(dt, 2), "s")

    # 6. Embed (text embedding) — classic /embed is multipart/form-data
    print("[6] Embed (text)")
    efields = {"model_name": "Marengo-retrieval-2.7", "text": "a rabbit in a forest"}
    mp, ctype = multipart_fields(efields)
    st, dt, resp = req("POST", "/embed", raw=mp, content_type=ctype)
    capture("06-embed.json", "POST", "/embed", efields, st, dt, resp)
    print("   /embed status", st, "in", round(dt, 2), "s")
    if st != 200:
        # fallback: try marengo3.0 model name
        efields2 = {"model_name": "marengo3.0", "text": "a rabbit in a forest"}
        mp, ctype = multipart_fields(efields2)
        st, dt, resp = req("POST", "/embed", raw=mp, content_type=ctype)
        capture("06-embed-alt.json", "POST", "/embed", efields2, st, dt, resp)
        print("   /embed (marengo3.0) status", st, "in", round(dt, 2), "s")

    save("00-run-summary.json", results)
    print("Done. Captures in api-captures/.")


if __name__ == "__main__":
    main()
