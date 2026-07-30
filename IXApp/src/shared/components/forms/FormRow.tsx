import React from 'react';
import { Grid } from '@mui/material';

export const FormRow: React.FC<{ children: React.ReactNode; spacing?: number }> = ({
  children,
  spacing = 2,
}) => {
  return (
    <Grid container spacing={spacing} sx={{ mb: 1.5 }}>
      {children}
    </Grid>
  );
};

export const FormColumn: React.FC<{
  children: React.ReactNode;
  xs?: number;
  sm?: number;
  md?: number;
  lg?: number;
}> = ({ children, xs = 12, sm = 6, md = 4, lg = 3 }) => {
  return (
    <Grid size={{ xs, sm, md, lg }}>
      {children}
    </Grid>
  );
};
