# Test fixture videos — provenance & attribution

Three 90-second clips, one per content category, **committed to the repo** so that golden
tests are reproducible by anyone who clones it. Total ~5.4 MB.

They exist because the original test assets could not do this job: the internal demo videos
are company-confidential and 293 MB, so they can be neither shared nor committed, which made
every golden test unreproducible outside the founder's laptop.

## 🚨 Why none of these comes from YouTube

**CLAUDE.md rule 8 forbids downloading video bytes from YouTube — no `yt-dlp`, no scraping,
ever.** A Creative Commons licence on the *content* does not override YouTube's Terms of
Service on the *delivery*: the licence and the ToS are separate instruments. The Phase 0.2
benchmark corpus is CC-licensed YouTube video, but it is only ever reached through Vertex
`fileData.fileUri` — Google fetches it, we never hold the bytes.

Fixtures are different: a fixture must live in the repo. So every file here is sourced from a
site that **publishes the media file itself under a licence permitting redistribution**.

Provenance was preferred over convenience: sources whose licence is stated at file or
organisation scope were chosen over ones where it is merely implied. EmacsConf's talks were
rejected for this reason — its only licence statement covers "material on the EmacsConf wiki",
which does not clearly extend to the speakers' video recordings. We sell compliance to DPOs;
our own test fixtures cannot rest on a licence we had to squint at.

## The files

| File | Category | Source | Licence |
|---|---|---|---|
| `slide_talk_fosdem_curl.mp4` | Slide-heavy conference talk — dense projected text, the OCR case | FOSDEM 2024 | CC BY 2.0 BE |
| `talking_head_nasa_bolten.mp4` | Talking head — essentially **no** on-screen text; the anti-hallucination case | NASA Goddard, via Wikimedia Commons | Public domain (US Gov) |
| `screencast_blender_lesson.mp4` | Screen recording — UI on screen, narrated; public analogue of the internal demo | Wikimedia Commons (own work) | CC BY-SA 4.0 |

⚠️ `screencast_blender_lesson.mp4` is **CC BY-SA**: share-alike attaches to that clip and to
adaptations *of it*. It does not reach vectorreel's source code, but if this ever becomes
uncomfortable, replace it — the clip is a test input, not a dependency.

## Attribution — required, and reproduced wherever a clip is shown

```
slide_talk_fosdem_curl.mp4
  "You too could have made curl!" by Daniel Stenberg, FOSDEM 2024.
  https://archive.fosdem.org/2024/schedule/event/fosdem-2024-1931-you-too-could-have-made-curl-/
  Licensed CC BY 2.0 BE — https://creativecommons.org/licenses/by/2.0/be/deed.en

talking_head_nasa_bolten.mp4
  NASA Earth Day 2023 interview with John Bolten, Chief of the Hydrological Sciences
  Laboratory. Credit: NASA's Goddard Space Flight Center — https://svs.gsfc.nasa.gov/14327
  Public domain (US Government work), via Wikimedia Commons.

screencast_blender_lesson.mp4
  "Module 3 Lesson 1 Screencast v2" by Lorenzoh, via Wikimedia Commons.
  https://commons.wikimedia.org/wiki/File:Module_3_Lesson_1_Screencast_v2.webm
  Licensed CC BY-SA 4.0 — https://creativecommons.org/licenses/by-sa/4.0/
```

## How they were produced

Re-encoded to 1280-wide H.264 / AAC to keep the repo small; the pipeline downsamples far
harder than this anyway (the public path runs at `MEDIA_RESOLUTION_LOW`), so nothing a test
depends on is lost.

`-map 0:v:0 -map 0:a:0` is load-bearing on the CCC/FOSDEM-style sources: they can carry
several audio tracks (live-translation feeds), and picking the track implicitly would make the
transcription non-deterministic.

```bash
# slide talk — ffmpeg reads over HTTP range requests, so only the needed bytes are fetched
ffmpeg -ss 00:12:00 -i "https://video.fosdem.org/2024/k1105/fosdem-2024-1931-you-too-could-have-made-curl-.mp4" \
  -t 90 -map 0:v:0 -map 0:a:0 -vf scale=1280:-2 \
  -c:v libx264 -crf 28 -preset veryfast -c:a aac -b:a 64k -movflags +faststart \
  slide_talk_fosdem_curl.mp4

# the two WebM sources do not seek reliably over HTTP — fetch, then trim with -ss AFTER -i
ffmpeg -i nasa.webm    -ss 00:01:00 -t 90 -map 0:v:0 -map 0:a:0 -vf scale=1280:-2 \
  -c:v libx264 -crf 28 -preset veryfast -c:a aac -b:a 64k -movflags +faststart \
  talking_head_nasa_bolten.mp4
ffmpeg -i blender.webm -ss 00:00:30 -t 90 -map 0:v:0 -map 0:a:0 -vf scale=1280:-2 \
  -c:v libx264 -crf 28 -preset veryfast -c:a aac -b:a 64k -movflags +faststart \
  screencast_blender_lesson.mp4
```
