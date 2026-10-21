namespace Bluey.Link;

// Local bounded metadata only: no LAN IDs, provider body, transcript or audio contents.
public sealed record AudioOutcome(long Attempt,string Code,long ElapsedMs,int InputBytes,int Characters)
{
    public static string Message(string code)=>code switch {
        "answer"=>"Gemini answer queued for the phone; desktop presentation awaits the phone.",
        "unavailable"=>"Gemini temporarily unavailable (503). Try again later.",
        "quota"=>"Gemini free-tier quota exhausted (429). Offline simulation is available.",
        "configuration"=>"Gemini configuration rejected (400). Update or review the request settings.",
        "credential"=>"Gemini credential or permission rejected (401/403). Review Windows key/project access.",
        "timeout"=>"Gemini request timed out.",
        "network"=>"Gemini network request failed. Check connectivity before authorizing again.",
        "empty"=>"Gemini response had no visible text; review thinking/output limits.",
        "budget"=>"Gemini thinking/output budget exhausted (MAX_TOKENS), with no visible text.",
        "safety"=>"Gemini blocked the response for safety. No answer displayed.",
        "malformed"=>"Gemini response format was rejected. No answer displayed.",
        "canceled"=>"Gemini turn canceled; any late result was discarded.",
        _=>"Gemini provider request failed. No answer displayed."
    };
}
