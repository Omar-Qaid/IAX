import React from 'react';
import { Breadcrumbs, Link, Typography, Box } from '@mui/material';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';
import HomeOutlinedIcon from '@mui/icons-material/HomeOutlined';
import { useLocation, Link as RouterLink } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { useRouteBreadcrumbs } from '@app/routes/useRouteBreadcrumbs';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const PageBreadcrumbs: React.FC = () => {
  const location = useLocation();
  const { t } = useAppTranslation();
  const breadcrumbs = useRouteBreadcrumbs();

  if (location.pathname === ROUTE_PATHS.DASHBOARD || location.pathname === '/') {
    return null;
  }

  return (
    <Box sx={{ mb: 1 }}>
      <Breadcrumbs
        separator={<NavigateNextIcon fontSize="small" sx={{ fontSize: '0.75rem' }} />}
        aria-label={t('common.breadcrumbs')}
        sx={{ fontSize: '0.75rem' }}
      >
        <Link
          component={RouterLink}
          underline="hover"
          color="inherit"
          to={ROUTE_PATHS.DASHBOARD}
          sx={{ display: 'flex', alignItems: 'center' }}
        >
          <HomeOutlinedIcon sx={{ fontSize: '0.9rem', marginInlineEnd: 0.5 }} />
          {t('nav.home')}
        </Link>
        {breadcrumbs.slice(1).map((item, index) => {
          const last = index === breadcrumbs.length - 2;
          const key = `${item.labelKey}-${index}`;

          return last || !item.path ? (
            <Typography
              key={key}
              color="text.primary"
              sx={{ fontSize: '0.75rem', fontWeight: 600 }}
            >
              {item.label ?? t(item.labelKey)}
            </Typography>
          ) : (
            <Link component={RouterLink} underline="hover" color="inherit" to={item.path} key={key}>
              {item.label ?? t(item.labelKey)}
            </Link>
          );
        })}
      </Breadcrumbs>
    </Box>
  );
};
