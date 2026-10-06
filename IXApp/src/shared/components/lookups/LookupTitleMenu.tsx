import React from 'react';
import { Button } from '@mui/material';
import { useInRouterContext, useNavigate } from 'react-router-dom';
import { lookupMasterRoute } from './lookupMasterRoutes';

interface LookupTitleMenuProps {
  name: string;
  label: string;
  masterRoute?: string;
}

export function LookupTitleMenu({ name, label, masterRoute }: LookupTitleMenuProps): React.ReactElement {
  const inRouter = useInRouterContext();
  const destination = lookupMasterRoute(name, masterRoute);
  if (!destination || !inRouter) return <>{label}</>;
  return <NavigableLookupTitle label={label} destination={destination} />;
}

function NavigableLookupTitle({ label, destination }: { label: string; destination: string }): React.ReactElement {
  const navigate = useNavigate();
  return <Button
    size="small"
    color="inherit"
    aria-label={`${label}: double-click to view details`}
    title="Double-click to view details"
    onMouseDown={(event) => { event.preventDefault(); event.stopPropagation(); }}
    onClick={(event) => event.stopPropagation()}
    onDoubleClick={(event) => { event.preventDefault(); event.stopPropagation(); navigate(destination); }}
    sx={{ minWidth: 0, minHeight: 0, p: 0, lineHeight: 'inherit', fontSize: 'inherit', fontWeight: 'inherit', textTransform: 'none', color: 'inherit', verticalAlign: 'baseline' }}
  >{label}</Button>;
}
