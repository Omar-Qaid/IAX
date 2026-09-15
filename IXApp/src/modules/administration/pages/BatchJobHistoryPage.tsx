import { useSearchParams } from 'react-router-dom';
import { BatchJobHistory } from '../components/BatchJobHistory';

export function BatchJobHistoryPage() {
  const [params] = useSearchParams();
  const id = Number(params.get('jobId'));
  return <BatchJobHistory jobId={Number.isSafeInteger(id) && id > 0 ? id : 0} />;
}
