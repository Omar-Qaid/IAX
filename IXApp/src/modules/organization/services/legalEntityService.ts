import { environment } from '@core/configuration/environment';
import { legalEntityMockRepository } from '../adapters/legalEntityMockRepository';
import { legalEntityApiRepository } from '../api/legalEntityApiRepository';

export const legalEntityService = environment.enableMockApi
  ? legalEntityMockRepository
  : legalEntityApiRepository;
