declare module 'utif' {
  interface UtifModule {
    encodeImage(
      rgba: ArrayBuffer,
      width: number,
      height: number,
      metadata?: Record<string, unknown>
    ): ArrayBuffer;
  }

  const UTIF: UtifModule;
  export default UTIF;
  export function encodeImage(
    rgba: ArrayBuffer,
    width: number,
    height: number,
    metadata?: Record<string, unknown>
  ): ArrayBuffer;
}
