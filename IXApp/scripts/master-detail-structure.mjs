/** One file map shared by generation and structural verification. */
export function masterDetailStructure(name) {
  if (!/^[A-Z][A-Za-z0-9]*$/.test(name)) {
    throw new Error('--name must be a PascalCase identifier.');
  }
  const camel = name[0].toLowerCase() + name.slice(1);
  return {
    page: `pages/${name}DetailsPage.tsx`,
    model: `details/${camel}Model.ts`,
    api: `api/${camel}Api.ts`,
    controller: `details/use${name}Document.ts`,
    header: `details/${name}HeaderSections.ts`,
    columns: `lines/${name}LineColumns.ts`,
    details: `details/${name}Details.tsx`,
    readme: 'README.md',
  };
}

export function masterDetailFeatureName(readme) {
  return /<!-- master-detail-feature: ([A-Z][A-Za-z0-9]*) -->/.exec(readme)?.[1];
}
