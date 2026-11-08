import '@testing-library/jest-dom';
import '@core/localization/i18n';

if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
  };
}

// Node 26 ships an experimental `localStorage` global that is undefined unless
// --localstorage-file is passed, and it shadows the one jsdom provides, so anything reading
// window.localStorage throws. Same guard shape as ResizeObserver above.
if (!globalThis.localStorage) {
  class MemoryStorage implements Storage {
    private values = new Map<string, string>();
    get length(): number {
      return this.values.size;
    }
    clear(): void {
      this.values.clear();
    }
    getItem(key: string): string | null {
      return this.values.has(key) ? (this.values.get(key) as string) : null;
    }
    key(index: number): string | null {
      return [...this.values.keys()][index] ?? null;
    }
    removeItem(key: string): void {
      this.values.delete(key);
    }
    setItem(key: string, value: string): void {
      this.values.set(key, String(value));
    }
  }
  for (const name of ['localStorage', 'sessionStorage'] as const)
    Object.defineProperty(globalThis, name, {
      configurable: true,
      writable: true,
      value: new MemoryStorage(),
    });
}
