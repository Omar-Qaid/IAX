const supported = new Set([
  'required', 'minlength', 'maxlength', 'exactlength', 'length', 'minvalue', 'maxvalue',
  'mindate', 'maxdate', 'range', 'regex', 'pattern', 'email', 'url', 'phone', 'saudimobile',
  'saudinationalid', 'saudiiban', 'taxnumber', 'passport', 'startswith', 'endswith', 'contains',
  'fileextensions', 'fileextension', 'allowedextensions', 'allowedtypes', 'filesize', 'maxfilesize',
  'minselected', 'maxselected', 'maxfiles', 'compare', 'comparison', 'crossfield', 'expression',
  'custom', 'customexpression', 'conditional', 'mask', 'inputmask', 'unique', 'uniqueglobal', 'uniqueperapplicant',
]);

export const supportedSubmissionRule = (type: string): boolean => supported.has(type);

// Same tokens as the server: 0/9 digit, A/a letter, * alphanumeric, backslash escape.
export function inputMaskValid(value: string, mask: string | null): boolean {
  if (!mask) return false;
  let index = 0;
  for (let position = 0; position < mask.length; position++) {
    if (index >= value.length) return false;
    const token = mask[position];
    const actual = value[index++];
    if (token === '\\') {
      if (++position >= mask.length || actual !== mask[position]) return false;
      continue;
    }
    const valid = token === '0' || token === '9' ? /^[0-9]$/.test(actual)
      : token === 'A' || token === 'a' ? /^[A-Za-z]$/.test(actual)
        : token === '*' ? /^[A-Za-z0-9]$/.test(actual) : actual === token;
    if (!valid) return false;
  }
  return index === value.length;
}
