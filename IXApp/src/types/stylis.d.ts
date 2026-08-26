declare module 'stylis' {
  import type { StylisElement, StylisPlugin } from '@emotion/cache';

  export type Element = StylisElement;
  export type Middleware = StylisPlugin;
  export const prefixer: Middleware;
}
