import React from 'react';
import { SalesOrderDetails } from '../../sales-order/details/SalesOrderDetails';
import { SALES_ORDER_V2_ROUTE_PATHS } from '../routes/salesOrderV2RoutePaths';

export function SalesOrderDetailsPageV2(): React.ReactElement {
  return <SalesOrderDetails detailPath={SALES_ORDER_V2_ROUTE_PATHS.detail} />;
}