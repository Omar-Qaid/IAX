# Shared constants

Defines UI density values (`uiDensity`), enterprise UI measurements (`enterpriseUiTokens`) and
font families (`fontFamilies`). Colors are not defined here: the enterprise surfaces live on the
theme as `theme.palette.enterprise` so they follow the theme mode and the primary color preset.
`enterpriseSurfaces` defines those surfaces and reads them back from a theme. Measurements that
describe a different control than a same-named `uiDensity` entry carry an explicit prefix
(`fieldControlHeight`, `detailGridRowHeight`, `detailGridHeaderHeight`). Core infrastructure constants remain in `core/constants`;
business-specific enums belong to modules.

Action identifiers, page modes and record states are **not** defined here: those files existed as
empty placeholders and were removed. Add them with the first feature that needs them.

[Shared](../README.md)
