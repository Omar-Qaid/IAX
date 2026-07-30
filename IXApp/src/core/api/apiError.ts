export interface ApiValidationProblem {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

export class ApiError extends Error {
  public readonly status: number;
  public readonly validationErrors: Record<string, string[]>;
  public readonly traceId?: string;
  public readonly originalProblem?: ApiValidationProblem;

  constructor(
    message: string,
    status: number = 500,
    validationErrors: Record<string, string[]> = {},
    traceId?: string,
    originalProblem?: ApiValidationProblem
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.validationErrors = validationErrors;
    this.traceId = traceId;
    this.originalProblem = originalProblem;

    Object.setPrototypeOf(this, ApiError.prototype);
  }

  public static fromProblem(problem: ApiValidationProblem, defaultStatus: number = 400): ApiError {
    const message = problem.title || problem.detail || 'Validation error occurred';
    return new ApiError(
      message,
      problem.status || defaultStatus,
      problem.errors || {},
      problem.traceId,
      problem
    );
  }
}
