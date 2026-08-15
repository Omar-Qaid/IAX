# Application configuration

Defines application-owned static configuration. `constants.ts` holds app constants, `featureFlags.ts` exposes UI feature switches, and `navigation.ts` declares the navigation hierarchy consumed by the shell. Runtime API configuration is instead owned by `core/configuration/environment.ts`.

When adding navigation, use an existing route constant and supply real permission metadata where required. Do not duplicate route strings here.

[App documentation](../README.md) · [Routing](../../../docs/routing-and-layouts.md)
