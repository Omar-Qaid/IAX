import { useAppTranslation } from '@core/localization/useAppTranslation';

/** Translated option lists of the sales order enum fields (D365 base enums). */
export function useSalesOrderOptions() {
  const { t } = useAppTranslation();
  const salesDeliveryTypeOptions = [
    { value: 0, label: t('salesOrder.deliveryTypes.none', 'None') },
    { value: 1, label: t('salesOrder.deliveryTypes.pickup', 'Pickup') },
    { value: 2, label: t('salesOrder.deliveryTypes.direct', 'Direct delivery') },
  ];
  const sourcingOriginOptions = [
    { value: 0, label: t('salesOrder.sourcingOrigin.none', 'None') },
    { value: 1, label: t('salesOrder.sourcingOrigin.direct', 'Direct') },
    { value: 2, label: t('salesOrder.sourcingOrigin.indirect', 'Indirect') },
  ];
  const deliveryDateControlOptions = [
    { value: 0, label: t('salesOrder.deliveryDateControl.none', 'None') },
    { value: 1, label: t('salesOrder.deliveryDateControl.salesLeadTime', 'Sales lead time') },
    { value: 2, label: t('salesOrder.ui.atp', 'ATP') },
    { value: 3, label: t('salesOrder.ui.ctp', 'CTP') },
  ];
  const ctpStatusOptions = [
    { value: 0, label: t('salesOrder.ctpStatus.none', 'None') },
    { value: 1, label: t('salesOrder.ctpStatus.complete', 'Complete') },
    { value: 2, label: t('salesOrder.ctpStatus.failed', 'Failed') },
  ];
  const carrierServiceOptions = [
    { value: 0, label: t('salesOrder.carrierService.none', 'None') },
    { value: 1, label: t('salesOrder.carrierService.ground', 'Ground') },
    { value: 2, label: t('salesOrder.carrierService.air', 'Air') },
    { value: 3, label: t('salesOrder.carrierService.ocean', 'Ocean') },
    { value: 4, label: t('salesOrder.carrierService.other', 'Other') },
  ];
  const reservationOptions = [
    { value: 0, label: t('salesOrder.reservationTypes.none', 'None') },
    { value: 1, label: t('salesOrder.reservationTypes.standard', 'Standard') },
    { value: 2, label: t('salesOrder.reservationTypes.sameLocation', 'Same location') },
    { value: 3, label: t('salesOrder.reservationTypes.sameWarehouse', 'Same warehouse') },
    { value: 4, label: t('salesOrder.reservationTypes.sameSite', 'Same site') },
  ];
  return {
    salesDeliveryTypeOptions,
    sourcingOriginOptions,
    deliveryDateControlOptions,
    ctpStatusOptions,
    carrierServiceOptions,
    reservationOptions,
  };
}
