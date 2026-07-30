import React from 'react';
import { Breadcrumbs, Link, Typography, Box } from '@mui/material';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';
import HomeOutlinedIcon from '@mui/icons-material/HomeOutlined';
import { useLocation, Link as RouterLink } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { capitalize } from '@core/utilities/stringUtils';

export const PageBreadcrumbs: React.FC = () => {
  const location = useLocation();
  const pathnames = location.pathname.split('/').filter((x) => x);

  if (location.pathname === ROUTE_PATHS.DASHBOARD || location.pathname === '/') {
    return null;
  }

  return (
    <Box sx={{ mb: 1 }}>
      <Breadcrumbs
        separator={<NavigateNextIcon fontSize="small" sx={{ fontSize: '0.75rem' }} />}
        aria-label="breadcrumb"
        sx={{ fontSize: '0.75rem' }}
      >
        <Link
          component={RouterLink}
          underline="hover"
          color="inherit"
          to={ROUTE_PATHS.DASHBOARD}
          sx={{ display: 'flex', alignItems: 'center' }}
        >
          <HomeOutlinedIcon sx={{ fontSize: '0.9rem', mr: 0.5 }} />
          Home
        </Link>
        {pathnames.map((value, index) => {
          const last = index === pathnames.length - 1;
          const to = `/${pathnames.slice(0, index + 1).join('/')}`;
          const formatted = capitalize(value.replace(/-/g, ' '));

          return last ? (
            <Typography key={to} color="text.primary" sx={{ fontSize: '0.75rem', fontWeight: 600 }}>
              {formatted}
            </Typography>
          ) : (
            <Link component={RouterLink} underline="hover" color="inherit" to={to} key={to}>
              {formatted}
            </Link>
          );
        })}
      </Breadcrumbs>
    </Box>
  );
};
