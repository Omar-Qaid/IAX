import React from 'react';
import { alpha, Box, Typography } from '@mui/material';

type ValueTone = 'neutral' | 'old' | 'new';

export interface RecordValueDisplayProps {
  value: unknown;
  tone?: ValueTone;
  strikeThrough?: boolean;
}

interface XmlField {
  label: string;
  value: string;
}

interface ParsedValue {
  kind: 'text' | 'xml' | 'xml-fields';
  text: string;
  fields?: XmlField[];
}

const firstText = (...values: Array<string | null | undefined>): string =>
  values.find((value) => value?.trim())?.trim() || '—';

const elementText = (element: Element, tagName: string): string | null =>
  element.getElementsByTagName(tagName)[0]?.textContent ?? null;

const decodeXmlText = (value: string): string =>
  value
    .replace(/^<!\[CDATA\[([\s\S]*)\]\]>$/, '$1')
    .replace(/&#x([0-9a-f]+);/gi, (_, code: string) => String.fromCodePoint(Number.parseInt(code, 16)))
    .replace(/&#(\d+);/g, (_, code: string) => String.fromCodePoint(Number(code)))
    .replaceAll('&lt;', '<')
    .replaceAll('&gt;', '>')
    .replaceAll('&quot;', '"')
    .replaceAll('&apos;', "'")
    .replaceAll('&amp;', '&')
    .trim();

const tagText = (value: string, tagName: string): string | null => {
  const match = value.match(new RegExp(`<${tagName}(?:\\s[^>]*)?>([\\s\\S]*?)<\\/${tagName}>`, 'i'));
  return match ? decodeXmlText(match[1].replace(/<[^>]+>/g, '')) : null;
};

const extractWorkflowFields = (value: string): XmlField[] =>
  Array.from(value.matchAll(/<Control(?:\s[^>]*)?>([\s\S]*?)<\/Control>/gi)).map(
    (match, index) => ({
      label: firstText(
        tagText(match[1], 'ControlLabelAR'),
        tagText(match[1], 'ControlLabel'),
        `Field ${index + 1}`
      ),
      value: firstText(
        tagText(match[1], 'ControlValueAR'),
        tagText(match[1], 'ControlValue'),
        tagText(match[1], 'ControlValueEN')
      ),
    })
  );

export const isWorkflowXmlValue = (value: unknown): boolean =>
  typeof value === 'string' && extractWorkflowFields(value).length > 0;

const prettyPrintXml = (value: string): string => {
  const lines = value
    .replace(/>\s*</g, '>\n<')
    .split('\n')
    .map((line) => line.trim())
    .filter(Boolean);
  let depth = 0;

  return lines
    .map((line) => {
      if (/^<\//.test(line)) depth = Math.max(0, depth - 1);
      const formatted = `${'  '.repeat(depth)}${line}`;
      const opensElement = /^<[^!?/][^>]*>/.test(line);
      const closesOnSameLine = /<\/[^>]+>$/.test(line);
      const selfClosing = /\/>$/.test(line);
      if (opensElement && !closesOnSameLine && !selfClosing) depth += 1;
      return formatted;
    })
    .join('\n');
};

const parseValue = (value: unknown): ParsedValue => {
  if (value == null || value === '') return { kind: 'text', text: '—' };
  if (typeof value === 'boolean') return { kind: 'text', text: value ? 'Yes' : 'No' };
  if (typeof value === 'object') {
    try {
      return { kind: 'text', text: JSON.stringify(value, null, 2) };
    } catch {
      return { kind: 'text', text: String(value) };
    }
  }

  const text = String(value).trim();
  const legacyFields = extractWorkflowFields(text);
  if (legacyFields.length > 0) {
    return { kind: 'xml-fields', text, fields: legacyFields };
  }
  if (!text.startsWith('<') || !text.endsWith('>') || typeof DOMParser === 'undefined') {
    return { kind: 'text', text };
  }

  const document = new DOMParser().parseFromString(text, 'application/xml');
  if (document.querySelector('parsererror')) return { kind: 'text', text };

  const controls = Array.from(document.getElementsByTagName('Control'));
  if (controls.length > 0) {
    return {
      kind: 'xml-fields',
      text,
      fields: controls.map((control, index) => ({
        label: firstText(
          elementText(control, 'ControlLabelAR'),
          elementText(control, 'ControlLabel'),
          `Field ${index + 1}`
        ),
        value: firstText(
          elementText(control, 'ControlValueAR'),
          elementText(control, 'ControlValue'),
          elementText(control, 'ControlValueEN')
        ),
      })),
    };
  }

  return { kind: 'xml', text: prettyPrintXml(text) };
};

export function RecordValueDisplay({
  value,
  tone = 'neutral',
  strikeThrough = false,
}: RecordValueDisplayProps): React.ReactElement {
  const parsed = parseValue(value);
  const toneColor = tone === 'old' ? 'error.main' : tone === 'new' ? 'success.main' : 'text.primary';

  if (parsed.kind === 'xml-fields') {
    return (
      <Box
        sx={{
          width: '100%',
          minWidth: 0,
          overflow: 'hidden',
          border: '1px solid',
          borderColor: 'divider',
          borderRadius: 1,
          bgcolor: tone === 'old'
            ? (theme) => alpha(theme.palette.error.main, 0.05)
            : tone === 'new'
              ? (theme) => alpha(theme.palette.success.main, 0.05)
              : 'background.paper',
        }}
      >
        {parsed.fields?.map((field, index) => (
          <Box
            key={`${field.label}-${index}`}
            sx={{
              display: 'grid',
              gridTemplateColumns: 'minmax(110px, 0.8fr) minmax(0, 1.2fr)',
              borderTop: index === 0 ? 0 : '1px solid',
              borderColor: 'divider',
            }}
          >
            <Typography sx={{ p: 0.75, fontSize: 11.5, fontWeight: 600, bgcolor: 'action.hover', overflowWrap: 'anywhere' }}>
              {field.label}
            </Typography>
            <Typography dir="auto" sx={{ p: 0.75, fontSize: 11.5, color: toneColor, overflowWrap: 'anywhere' }}>
              {field.value}
            </Typography>
          </Box>
        ))}
      </Box>
    );
  }

  return (
    <Typography
      component={parsed.kind === 'xml' ? 'pre' : 'span'}
      sx={{
        m: 0,
        display: 'block',
        minWidth: 0,
        maxWidth: '100%',
        fontFamily: parsed.kind === 'xml' ? 'monospace' : 'inherit',
        fontSize: parsed.kind === 'xml' ? 11.5 : 'inherit',
        lineHeight: parsed.kind === 'xml' ? 1.45 : 'inherit',
        whiteSpace: parsed.kind === 'xml' ? 'pre-wrap' : 'normal',
        overflowWrap: 'anywhere',
        textDecoration: strikeThrough && parsed.text !== 'NULL' ? 'line-through' : 'none',
        color: toneColor,
      }}
    >
      {parsed.text}
    </Typography>
  );
}
