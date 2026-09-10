import { beforeEach, expect, it, vi } from 'vitest';
const calls = vi.hoisted(() => ({ get: vi.fn(), put: vi.fn(), delete: vi.fn() }));
vi.mock('@core/api/apiClient', () => ({ apiClient: calls }));
import { notificationInboxApi } from '@app/shell/notificationInboxApi';

beforeEach(() => {
  vi.resetAllMocks();
  calls.get.mockResolvedValue({ data: { success: true, data: [], pagination: { totalPages: 3 } } });
  calls.put.mockResolvedValue({ data: { success: true, data: true } });
  calls.delete.mockResolvedValue({ data: { success: true, data: true } });
});

it('requests paged current-user archives without supplying another user identity', async () => {
  expect(await notificationInboxApi.list(2, 2)).toEqual({ items: [], totalPages: 3 });
  expect(calls.get).toHaveBeenCalledWith('/v1/SysNotification', expect.objectContaining({ params: { pageNumber: 2, pageSize: 20, isArchived: true } }));
});
it('requests the unread tab through the server filter', async () => {
  await notificationInboxApi.list(1, 1);
  expect(calls.get).toHaveBeenCalledWith('/v1/SysNotification', expect.objectContaining({ params: { pageNumber: 1, pageSize: 20, isArchived: false, isRead: false } }));
});
it('uses the existing read/archive/delete routes and rejects unsuccessful responses', async () => {
  await notificationInboxApi.update('read', '7');
  await notificationInboxApi.update('archive', '7');
  await notificationInboxApi.update('read-all');
  await notificationInboxApi.update('delete', '7');
  expect(calls.put.mock.calls.map(([path]) => path)).toEqual(['/v1/SysNotification/7/read', '/v1/SysNotification/7/archive', '/v1/SysNotification/read-all']);
  expect(calls.delete).toHaveBeenCalledWith('/v1/SysNotification/7');
  calls.put.mockResolvedValue({ data: { success: false, data: false, message: 'Denied' } });
  await expect(notificationInboxApi.update('read', '7')).rejects.toThrow('Denied');
});
