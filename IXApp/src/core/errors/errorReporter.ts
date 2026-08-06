export interface ReportedErrorContext {
  source: string;
  componentStack?: string | null;
  correlationId?: string;
}

export interface ErrorReporter {
  report(error: Error, context: ReportedErrorContext): void;
}

class ConsoleErrorReporter implements ErrorReporter {
  report(error: Error, context: ReportedErrorContext): void {
    console.error(`[${context.source}]`, error, context);
  }
}

/** Replace this adapter when an external telemetry provider is configured. */
export const errorReporter: ErrorReporter = new ConsoleErrorReporter();
