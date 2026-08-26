# Assets

## Overview

Static files imported by frontend source code. An asset's presence does not mean it is used by a production page.

The Arabic web-font assets are registered once in `fonts.css` and exposed through the direction-aware application theme. Arabic/RTL and English/LTR layouts retain separate font preferences. See `SAUDI-FONT-NOTICE.md`, `FS-ALBERT-FONT-NOTICE.md`, and `OPEN-FONT-NOTICE.md` for ownership and license information.

Use `public` for files that require stable public URLs and this folder for assets processed by Vite imports. Prefer the existing theme and Material UI icons for interface chrome instead of adding decorative files here.

## Related documentation

- [Source map](../README.md)
- [UI/UX and responsive standards](../../docs/ui-ux-and-responsive.md)
