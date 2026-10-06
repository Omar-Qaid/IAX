import React from 'react';
import { Box, Button, IconButton, Tooltip } from '@mui/material';
import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
import { useInRouterContext, useNavigate } from 'react-router-dom';
import { useFieldMasterRoute } from '@shared/components/fields/FieldPresentationContext';

interface LookupNavigationLabelProps {
  label: string;
  masterRoute?: string;
}

export function LookupNavigationLabel({ label, masterRoute }: LookupNavigationLabelProps): React.ReactElement {
  const inRouter = useInRouterContext();
  const inheritedRoute = useFieldMasterRoute();
  const destination = (masterRoute ?? inheritedRoute)?.trim();
  if (!destination || !inRouter) return <>{label}</>;
  return <NavigableLabel label={label} destination={destination} />;
}

function NavigableLabel({ label, destination }: { label: string; destination: string }): React.ReactElement {
  const navigate = useNavigate();
  const stopMouseDown = (event: React.MouseEvent) => {
    event.preventDefault();
    event.stopPropagation();
  };

  return (
    <Box component="span" sx={{ display: 'inline-flex', alignItems: 'center', gap: '3px', maxWidth: '100%', verticalAlign: 'middle' }}>
      <Button
        size="small"
        color="inherit"
        aria-label={`${label}: double-click to view details`}
        title="Double-click to view details"
        onMouseDown={stopMouseDown}
        onClick={(event) => event.stopPropagation()}
        onDoubleClick={(event) => { event.preventDefault(); event.stopPropagation(); navigate(destination); }}
        sx={{ minWidth: 0, minHeight: 0, p: 0, lineHeight: 'inherit', fontSize: 'inherit', fontWeight: 'inherit', textTransform: 'none', color: 'inherit', verticalAlign: 'baseline' }}
      >
        {label}
      </Button>
      <Tooltip title="Open details" describeChild>
        <IconButton
          size="small"
          aria-label={`Open ${label} details`}
          onMouseDown={stopMouseDown}
          onClick={(event) => { event.preventDefault(); event.stopPropagation(); navigate(destination); }}
          sx={{ width: 16, height: 16, minWidth: 16, minHeight: 16, p: 0, flex: '0 0 auto', color: 'text.secondary', verticalAlign: 'middle', '&:hover': { color: 'primary.main' } }}
        >
          <OpenInNewOutlinedIcon sx={{ fontSize: 11 }} />
        </IconButton>
      </Tooltip>
    </Box>
  );
}
