import { afterEach, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { dynamicRequestFormApi } from '@modules/workflow/api/dynamicRequestFormApi';

afterEach(() => vi.restoreAllMocks());

it('uploads files and their field ownership in one submission request', async () => {
  const post = vi.spyOn(apiClient, 'post').mockResolvedValue({ data: { success: true, data: { requestId: 1 } } });
  const file = new File(['hello'], 'hello.txt');
  await dynamicRequestFormApi.submit({ processId: 7, values: [], optionFeatureValues: [] }, [{ file, requestControlId: 10, optionId: 20 }]);
  expect(post).toHaveBeenCalledTimes(1);
  const [url, data] = post.mock.calls[0];
  expect(url).toBe('/v1/WfRequest/submit-with-files');
  expect(data).toBeInstanceOf(FormData);
  expect((data as FormData).getAll('files')).toEqual([file]);
  expect(JSON.parse((data as FormData).get('submission') as string)).toEqual({
    processId: 7, values: [], optionFeatureValues: [], uploadTargets: [{ requestControlId: 10, optionId: 20 }],
  });
});
