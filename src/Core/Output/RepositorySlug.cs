using System.Globalization;
using System.Text;

namespace MdReel.Core.Output;

/// <summary>
/// The §4b naming rules. Deterministic by contract: the same documents plus the same curation
/// metadata must produce the same repository bytes, so every identifier here is a pure function of
/// its input.
/// </summary>
public static class RepositorySlug
{
    /// <summary>Lowercase-hyphenated ASCII, per §4b.</summary>
    public static string Slugify(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // Decompose so accented Latin folds to its base letter rather than being deleted:
        // "Grönlund" must become "gronlund", not "grnlund".
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else if (ch is ' ' or '-' or '_' or '/' or '.' or ':' or ',')
            {
                builder.Append('-');
            }
        }

        // Collapse runs and trim, so "A — B" does not become "a---b".
        var collapsed = new StringBuilder(builder.Length);
        var previousHyphen = true; // leading hyphens are dropped
        foreach (var ch in builder.ToString())
        {
            if (ch == '-')
            {
                if (!previousHyphen)
                {
                    collapsed.Append('-');
                }

                previousHyphen = true;
                continue;
            }

            collapsed.Append(ch);
            previousHyphen = false;
        }

        return collapsed.ToString().Trim('-');
    }

    /// <summary>
    /// <c>&lt;yyyy-mm-dd&gt;-&lt;title-slug&gt;</c>. The date is the recording/publication date when
    /// known, else the processed date — a session's identity should not change because we
    /// reprocessed it.
    /// </summary>
    public static string ForSession(string title, string date) =>
        $"{date}-{Slugify(title)}";

    /// <summary>
    /// §4b collision rule: suffix <c>-2</c>, <c>-3</c>, … Deterministic on input order, so a
    /// regenerated repository does not reshuffle its own filenames.
    /// </summary>
    public static string Deduplicate(string slug, ISet<string> taken)
    {
        ArgumentNullException.ThrowIfNull(taken);

        if (taken.Add(slug))
        {
            return slug;
        }

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"{slug}-{suffix}";
            if (taken.Add(candidate))
            {
                return candidate;
            }
        }
    }

    /// <summary>
    /// The GitHub auto-anchor of a §4 section heading — lowercase, punctuation stripped, spaces to
    /// hyphens. <c>## [00:03:40] Invoice workflow</c> → <c>#000340-invoice-workflow</c>.
    ///
    /// The visible timestamp beside it is mandatory in the citation grammar precisely because
    /// anchors are a rendering detail that can stop resolving; a timestamp stays checkable against
    /// the video regardless.
    /// </summary>
    public static string GithubAnchor(string timestamp, string heading)
    {
        ArgumentNullException.ThrowIfNull(timestamp);
        ArgumentNullException.ThrowIfNull(heading);

        var text = $"[{timestamp}] {heading}".ToLowerInvariant();
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (char.IsAsciiLetterOrDigit(ch) || ch == '-')
            {
                // ⚠️ Existing hyphens SURVIVE. GitHub strips punctuation but keeps hyphens, so
                // "AI-Infused" anchors as `ai-infused`, not `aiinfused`. Dropping them produced
                // anchors that silently did not resolve — invisible until a real heading contained
                // a hyphen, which the canonical fixture happened not to.
                builder.Append(ch);
            }
            else if (ch == ' ')
            {
                builder.Append('-');
            }
        }

        return builder.ToString();
    }
}
