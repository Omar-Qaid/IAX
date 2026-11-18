export const accountContactsRoute = '/contacts/:kind/:accountNum';
export const contactDetailsRoute = '/contacts/:kind/:accountNum/:contactId';
export function accountContacts(kind: string, account: string, id?: string) {
  const path = `/contacts/${kind}/${encodeURIComponent(account)}`;
  return id ? `${path}/${encodeURIComponent(id)}` : path;
}
