namespace IAX.IXApi.Modules.Workflow.Requests;

public static class InputMaskRule
{
    // 0/9 = digit, A/a = ASCII letter, * = ASCII letter or digit;
    // all other characters are literals. Backslash escapes a token.
    public static bool IsValid(string value, string? mask)
    {
        if (string.IsNullOrEmpty(mask)) return false;
        var index = 0;
        for (var position = 0; position < mask.Length; position++)
        {
            if (index >= value.Length) return false;
            var token = mask[position];
            var actual = value[index++];
            if (token == '\\')
            {
                if (++position >= mask.Length || actual != mask[position]) return false;
                continue;
            }
            var valid = token switch
            {
                '0' or '9' => char.IsAsciiDigit(actual),
                'A' or 'a' => char.IsAsciiLetter(actual),
                '*' => char.IsAsciiLetterOrDigit(actual),
                _ => actual == token
            };
            if (!valid) return false;
        }
        return index == value.Length;
    }
}
