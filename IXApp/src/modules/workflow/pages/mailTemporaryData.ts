import type { WfRequestRecord } from '../api/wfRequestApi';

export interface MailAssignment {
  id: string;
  activity: string;
  step: string;
  assignee: string;
  assignedAt: string;
  finishedAt: string | null;
  status: 'Pending' | 'Completed' | 'Stopped';
  score: number;
}

export interface MailProcessData {
  id: string;
  assignmentId: string;
  finishedAt: string | null;
  activityDetails: string;
  extendedProperties: string;
}

export interface MailProcessDataDetail {
  id: string;
  label: string;
  value: string;
}

export interface MailHistoryEntry {
  id: string;
  title: string;
  subtitle: string;
  actor: string;
  action: string;
  date: string;
  details: string;
  current: boolean;
  completed: boolean;
}

export interface TemporaryMailDetails {
  assignment: MailAssignment;
  processData: MailProcessData;
  processDataDetails: MailProcessDataDetail[];
  history: MailHistoryEntry[];
}

const requestStatus = (request: WfRequestRecord): MailAssignment['status'] => {
  if (request.isStopped) return 'Stopped';
  if (request.isFinished) return 'Completed';
  return 'Pending';
};

/**
 * Temporary projection used until workflow execution exposes assignment and process-data APIs.
 * Keeping it outside the page makes the future API replacement a single-boundary change.
 */
export const getTemporaryMailDetails = (request: WfRequestRecord): TemporaryMailDetails => {
  const assignmentId = `assignment-${request.recId}`;
  const status = requestStatus(request);
  const requestText = request.requestDetails || request.description || request.notes || 'No details provided.';

  return {
    assignment: {
      id: assignmentId,
      activity: request.isFinished ? 'Request completed' : 'Review request',
      step: request.isFinished ? 'Final stage' : 'Current stage',
      assignee: request.employeeId ? `Employee ${request.employeeId}` : 'Workflow queue',
      assignedAt: request.requestDate,
      finishedAt: request.finishedDate,
      status,
      score: request.score,
    },
    processData: {
      id: `process-data-${request.recId}`,
      assignmentId,
      finishedAt: request.finishedDate,
      activityDetails: requestText,
      extendedProperties: request.notes || '—',
    },
    processDataDetails: [
      { id: 'request-code', label: 'Request number', value: request.code || `#${request.recId}` },
      { id: 'requester', label: 'Requester', value: request.employeeId ? `Employee ${request.employeeId}` : 'Not assigned' },
      { id: 'company', label: 'Company', value: request.dataAreaId || '—' },
      { id: 'version', label: 'Version', value: String(request.recVersion) },
    ],
    history: [
      {
        id: 'current',
        title: request.isFinished ? 'Request completed' : request.isStopped ? 'Request stopped' : 'Execution department',
        subtitle: request.isFinished ? 'Final stage' : 'Current stage',
        actor: request.employeeId ? `Employee ${request.employeeId}` : 'Human Resources',
        action: request.isFinished ? 'Approve transaction' : request.isStopped ? 'Stop transaction' : 'Apply administrative action',
        date: request.finishedDate || request.stoppedDate || request.requestDate,
        details: request.notes || requestText,
        current: !request.isFinished && !request.isStopped,
        completed: request.isFinished,
      },
      {
        id: 'assigned',
        title: 'Department manager',
        subtitle: 'Approved',
        actor: request.employeeId ? `Employee ${request.employeeId}` : 'Direct manager',
        action: 'Approval completed',
        date: request.requestDate,
        details: 'The request was reviewed, approved, and forwarded to the next stage.',
        current: false,
        completed: true,
      },
      {
        id: 'submitted',
        title: 'First step',
        subtitle: request.code || `Request ${request.recId}`,
        actor: request.employeeId ? `Employee ${request.employeeId}` : 'Requester',
        action: 'Submit transaction',
        date: request.requestDate,
        details: request.description || 'The workflow request was created and submitted successfully.',
        current: false,
        completed: true,
      },
    ],
  };
};
