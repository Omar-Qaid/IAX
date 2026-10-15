import React, { useMemo, useState } from 'react';
import { Alert, Box, Button, Divider, Paper, Step, StepLabel, Stepper, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { PageContainer } from '@shared/components/page/PageContainer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { localizedName } from '@shared/utilities/localizedName';
import { ecoResProductApi, type EcoResProductRecord } from '../api/ecoResProductApi';
import { releasedProductApi, type ReleaseCompanyRecord } from '../api/releasedProductApi';
import { INVENTORY_ROUTE_PATHS } from '../routes/inventoryRoutePaths';

const productColumns: ColumnDef<EcoResProductRecord>[] = [
  { field: 'name', headerName: 'Product name', width: 240 }, { field: 'productNumber', headerName: 'Product number', width: 170 },
  { field: 'nameAlias', headerName: 'Arabic name', width: 240 }, { field: 'productType', headerName: 'Product type', width: 130 },
  { field: 'productSubtype', headerName: 'Product subtype', width: 140 },
];
const companyColumns: ColumnDef<ReleaseCompanyRecord>[] = [
  { field: 'code', headerName: 'Company', width: 150 }, { field: 'name', headerName: 'Legal entity name', width: 300 }, { field: 'nameAlias', headerName: 'Arabic name', width: 260 },
];

export function EcoResProductReleasePage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation(); const navigate = useNavigate();
  const [step, setStep] = useState(0); const [productIds, setProductIds] = useState<string[]>([]); const [companyIds, setCompanyIds] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false); const [error, setError] = useState<string | null>(null); const [completed, setCompleted] = useState(false);
  const products = useQuery({ queryKey: ['inventory', 'shared-products'], queryFn: ({ signal }) => ecoResProductApi.list(signal) });
  const companies = useQuery({ queryKey: ['inventory', 'release-companies'], queryFn: ({ signal }) => releasedProductApi.companies(signal) });
  const selectedProducts = useMemo(() => (products.data ?? []).filter((row) => productIds.includes(row.id)), [productIds, products.data]);
  const selectedCompanies = useMemo(() => (companies.data ?? []).filter((row) => companyIds.includes(row.id)), [companyIds, companies.data]);
  const steps = [t('productRelease.steps.products', 'Select products to release'), t('productRelease.steps.variants', 'Select product variants to release'), t('productRelease.steps.companies', 'Select companies to release to'), t('productRelease.steps.confirm', 'Confirm selection')];
  const canNext = step === 0 ? productIds.length > 0 : step === 2 ? companyIds.length > 0 : true;
  const finish = async () => {
    setSubmitting(true); setError(null);
    try {
      for (const company of selectedCompanies) for (const product of selectedProducts) await releasedProductApi.releaseToCompany({ productId: product.id, itemNumber: product.productNumber, nameAlias: product.searchName }, company.code);
      setCompleted(true);
    } catch (reason) { setError(reason instanceof Error ? reason.message : t('errors.generic')); }
    finally { setSubmitting(false); }
  };
  return <PageContainer sx={{ minHeight: 'calc(100vh - 84px)', gap: 0 }}>
    <Box sx={{ px: 2, pt: 1 }}><Typography variant="caption">{t('common.standardView')}</Typography><Typography variant="h5" sx={{ fontSize: '1.2rem', fontWeight: 600 }}>{t('productRelease.title', 'Release products')}</Typography></Box>
    <Divider sx={{ mt: 1 }} />
    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '230px minmax(0, 1fr)' }, flex: 1, minHeight: 0 }}>
      <Box sx={{ borderInlineEnd: 1, borderColor: 'divider', p: 2 }}><Stepper activeStep={step} orientation="vertical">{steps.map((label) => <Step key={label}><StepLabel>{label}</StepLabel></Step>)}</Stepper></Box>
      <Box sx={{ p: 2, minWidth: 0 }}>
        {completed ? <Alert severity="success">{t('productRelease.completed', 'The selected products were released successfully.')}</Alert> : <>
          {step === 0 && <WizardGrid title={steps[0]} rows={products.data ?? []} columns={productColumns} loading={products.isLoading} selectedIds={productIds} onSelectionChange={setProductIds} />}
          {step === 1 && <Paper variant="outlined" sx={{ p: 2 }}><Typography sx={{ fontWeight: 600 }}>{steps[1]}</Typography><Alert severity="info" sx={{ mt: 2 }}>{t('productRelease.noVariants', 'The selected products use subtype Product and do not have product variants. Continue to company selection.')}</Alert></Paper>}
          {step === 2 && <WizardGrid title={steps[2]} rows={(companies.data ?? []).map((row) => ({ ...row, name: localizedName(row, isRtl) }))} columns={companyColumns} loading={companies.isLoading} selectedIds={companyIds} onSelectionChange={setCompanyIds} />}
          {step === 3 && <Paper variant="outlined" sx={{ p: 2 }}><Typography sx={{ fontWeight: 600 }}>{steps[3]}</Typography><Typography sx={{ mt: 2 }}>{t('productRelease.productsCount', { count: selectedProducts.length, defaultValue: `${selectedProducts.length} product(s)` })}</Typography><Typography>{t('productRelease.companiesCount', { count: selectedCompanies.length, defaultValue: `${selectedCompanies.length} company/companies` })}</Typography><Box sx={{ mt: 2 }}>{selectedProducts.map((product) => <Typography key={product.id} variant="body2">{product.productNumber} — {localizedName(product, isRtl)}</Typography>)}</Box><Box sx={{ mt: 1 }}>{selectedCompanies.map((company) => <Typography key={company.id} variant="body2">{company.code} — {localizedName(company, isRtl)}</Typography>)}</Box></Paper>}
          {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
        </>}
      </Box>
    </Box>
    <Box sx={{ display: 'flex', gap: 1, borderTop: 1, borderColor: 'divider', px: 2, py: 1, justifyContent: 'flex-start' }}>
      {completed ? <Button variant="contained" onClick={() => navigate(INVENTORY_ROUTE_PATHS.RELEASED_PRODUCTS)}>{t('actions.finish', 'Finish')}</Button> : <>
        <Button disabled={step === 0 || submitting} onClick={() => setStep((value) => value - 1)}>{t('actions.back', 'Back')}</Button>
        {step < steps.length - 1 ? <Button variant="contained" disabled={!canNext} onClick={() => setStep((value) => value + 1)}>{t('actions.next', 'Next')}</Button> : <Button variant="contained" disabled={submitting} onClick={finish}>{t('actions.finish', 'Finish')}</Button>}
        <Button variant="outlined" disabled={submitting} onClick={() => navigate(INVENTORY_ROUTE_PATHS.PRODUCTS)}>{t('actions.cancel')}</Button>
      </>}
    </Box>
  </PageContainer>;
}

function WizardGrid<T extends { id: string }>({ title, rows, columns, loading, selectedIds, onSelectionChange }: { title: string; rows: T[]; columns: ColumnDef<T>[]; loading: boolean; selectedIds: string[]; onSelectionChange: (ids: string[]) => void }): React.ReactElement {
  return <Paper variant="outlined" sx={{ p: 1.5, maxWidth: 1000 }}><Typography sx={{ mb: 1, fontWeight: 600 }}>{title}</Typography><DataGrid rows={rows} columns={columns} loading={loading} selectionMode="multiple" checkboxSelection selectedIds={selectedIds} onSelectionChange={(selection) => onSelectionChange(selection.filter((id): id is string => typeof id === 'string'))} height="calc(100vh - 260px)" hideSidebar hideToolbar storageKey={`inventory.product-release.${title}`} /></Paper>;
}
