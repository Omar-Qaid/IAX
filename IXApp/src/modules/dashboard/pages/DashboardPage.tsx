import React from 'react';
import { WorkspacePage } from '@patterns/workspace/WorkspacePage';
import { WorkspaceTile } from '@patterns/workspace/WorkspaceTile';
import { Grid, Paper, Typography, Box, List, ListItem, ListItemText, Divider, Button } from '@mui/material';
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined';
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';
import TrendingUpOutlinedIcon from '@mui/icons-material/TrendingUpOutlined';
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import { useNavigate } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { StatusBadge } from '@shared/components/status/StatusBadge';

export const DashboardPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <WorkspacePage title="Operations Dashboard" subtitle="Enterprise Workspace & Financial Highlights">
      {/* KPI Tiles Row */}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <WorkspaceTile
            title="Total Customers"
            value="148"
            subtitle="+12 this month"
            icon={<PeopleAltOutlinedIcon fontSize="large" />}
            color="primary"
            onClick={() => navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <WorkspaceTile
            title="Open Sales Orders"
            value="34"
            subtitle="$420,500 pending"
            icon={<ShoppingCartOutlinedIcon fontSize="large" />}
            color="info"
            onClick={() => navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <WorkspaceTile
            title="Monthly Sales"
            value="$1,245,000"
            subtitle="+8.4% vs last month"
            icon={<TrendingUpOutlinedIcon fontSize="large" />}
            color="success"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <WorkspaceTile
            title="Overdue Balance"
            value="$45,200"
            subtitle="4 critical accounts"
            icon={<WarningAmberOutlinedIcon fontSize="large" />}
            color="warning"
          />
        </Grid>
      </Grid>

      {/* Main Content Grid: Recent Orders + Quick Actions */}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper elevation={0} sx={{ p: 2, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}` }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1.5 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                Recent Sales Orders
              </Typography>
              <Button
                size="small"
                endIcon={<ArrowForwardIcon fontSize="small" />}
                onClick={() => navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS)}
              >
                View All Orders
              </Button>
            </Box>
            <List disablePadding>
              <ListItem sx={{ py: 1, px: 0 }}>
                <ListItemText
                  primary={<Typography variant="body2" sx={{ fontWeight: 700 }}>SO-00101 - Contoso Retail Americas</Typography>}
                  secondary={<Typography variant="caption">Date: 2025-07-01 | Items: 2 lines</Typography>}
                />
                <Box sx={{ textAlign: 'right', mr: 2 }}>
                  <Typography variant="body2" sx={{ fontWeight: 700 }}>$12,650.00</Typography>
                  <StatusBadge status="open" />
                </Box>
              </ListItem>
              <Divider />
              <ListItem sx={{ py: 1, px: 0 }}>
                <ListItemText
                  primary={<Typography variant="body2" sx={{ fontWeight: 700 }}>SO-00102 - Fabrikam Supplies Ltd.</Typography>}
                  secondary={<Typography variant="caption">Date: 2025-07-05 | Items: 1 line</Typography>}
                />
                <Box sx={{ textAlign: 'right', mr: 2 }}>
                  <Typography variant="body2" sx={{ fontWeight: 700 }}>$4,950.00</Typography>
                  <StatusBadge status="confirmed" />
                </Box>
              </ListItem>
            </List>
          </Paper>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Paper elevation={0} sx={{ p: 2, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}` }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 1 }}>
              Quick Tasks
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mt: 1 }}>
              <Button
                variant="outlined"
                fullWidth
                size="small"
                onClick={() => navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS)}
                sx={{ justifyContent: 'flex-start' }}
              >
                Create New Customer
              </Button>
              <Button
                variant="outlined"
                fullWidth
                size="small"
                onClick={() => navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS)}
                sx={{ justifyContent: 'flex-start' }}
              >
                Create Sales Order
              </Button>
              <Button
                variant="outlined"
                fullWidth
                size="small"
                onClick={() => navigate(ROUTE_PATHS.FOUNDATION.CURRENCIES)}
                sx={{ justifyContent: 'flex-start' }}
              >
                Manage Exchange Rates
              </Button>
              <Button
                variant="outlined"
                fullWidth
                size="small"
                onClick={() => navigate(ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS)}
                sx={{ justifyContent: 'flex-start' }}
              >
                System Configuration
              </Button>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </WorkspacePage>
  );
};
