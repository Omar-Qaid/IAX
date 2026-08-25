// Compatibility export for application-shell consumers. Company context belongs to
// core so feature modules can read it without depending on the application layer.
export { useCompanyStore as useAppStore } from '@core/company/useCompanyStore';
