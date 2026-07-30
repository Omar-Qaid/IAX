import React, { useState, useMemo, useCallback, useEffect, useRef } from 'react';
import {
    Dialog,
    DialogContent,
    InputBase,
    Typography,
    Box,
    Grid,
    Chip,
    Paper,
    alpha,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import SearchIcon from '@mui/icons-material/Search';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import SettingsIcon from '@mui/icons-material/Settings';
import GroupWorkIcon from '@mui/icons-material/GroupWork';
import { useTranslation } from 'react-i18next';
import { useUIStore } from '../../store/uiStore';

// ─── Page definitions with icons and categories ──────────────────────────────

interface PageItem {
    id: string;
    label: string; // Translation key
    path: string;
    icon: React.ReactNode;
    category: 'Application' | 'Table';
}

const iconSx = { fontSize: 24, color: 'text.secondary' };

const ALL_PAGES: PageItem[] = [
    // Application pages
    { id: 'dashboard', label: 'nav.dashboard', path: '/dashboard', icon: <DashboardIcon sx={iconSx} />, category: 'Application' },
    { id: 'settings', label: 'nav.settings', path: '/settings', icon: <SettingsIcon sx={iconSx} />, category: 'Application' },

    // Accounts Receivable
    { id: 'customers', label: 'nav.customers', path: '/customers', icon: <PeopleIcon sx={iconSx} />, category: 'Table' },
    { id: 'customer-groups', label: 'nav.customer_groups', path: '/customer-groups', icon: <GroupWorkIcon sx={iconSx} />, category: 'Table' },
    { id: 'pos', label: 'nav.pos', path: '/pointOfSale', icon: <ShoppingCartIcon sx={iconSx} />, category: 'Application' },
];

const CATEGORIES = ['Application', 'Table'] as const;


// ─── Page Card ───────────────────────────────────────────────────────────────

const PageCard: React.FC<{ page: PageItem; onClick: () => void }> = React.memo(({ page, onClick }) => {
    const { t } = useTranslation();
    return (
        <Paper
            elevation={0}
            onClick={onClick}
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                gap: 1,
                p: 2,
                borderRadius: '2px',
                border: '1px solid',
                borderColor: 'divider',
                cursor: 'pointer',
                transition: 'all 0.15s ease',
                minHeight: 88,
                '&:hover': {
                    bgcolor: (theme) => alpha(theme.palette.primary.main, 0.06),
                    borderColor: 'primary.main',
                },
            }}
        >
            {page.icon}
            <Typography
                variant="caption"
                sx={{
                    fontSize: '0.75rem',
                    fontWeight: 500,
                    color: 'text.primary',
                    textAlign: 'center',
                    lineHeight: 1.3,
                }}
            >
                {t(page.label)}
            </Typography>
        </Paper>
    );
});

// ─── Command Palette ─────────────────────────────────────────────────────────

const CommandPalette: React.FC = () => {
    const { t } = useTranslation();
    const open = useUIStore((s) => s.commandPaletteOpen);
    const setOpen = useUIStore((s) => s.setCommandPaletteOpen);
    const addRecentPage = useUIStore((s) => s.addRecentPage);
    const navigate = useNavigate();
    const inputRef = useRef<HTMLInputElement>(null);

    const [query, setQuery] = useState('');

    const handleDialogEntered = useCallback(() => {
        setQuery('');
        inputRef.current?.focus();
    }, []);

    // Ctrl+K shortcut
    useEffect(() => {
        const handler = (e: KeyboardEvent) => {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                setOpen(!open);
            }
        };
        window.addEventListener('keydown', handler);
        return () => window.removeEventListener('keydown', handler);
    }, [open, setOpen]);

    const filteredPages = useMemo(() => {
        const q = query.toLowerCase().trim();
        if (!q) return ALL_PAGES;
        return ALL_PAGES.filter(
            (p) =>
                t(p.label).toLowerCase().includes(q) ||
                p.path.toLowerCase().includes(q) ||
                p.category.toLowerCase().includes(q)
        );
    }, [query, t]);

    const groupedPages = useMemo(() => {
        const groups: Record<string, PageItem[]> = {};
        for (const cat of CATEGORIES) {
            const items = filteredPages.filter((p) => p.category === cat);
            if (items.length > 0) groups[cat] = items;
        }
        return groups;
    }, [filteredPages]);

    const handleSelect = useCallback(
        (page: PageItem) => {
            setOpen(false);
            addRecentPage(page.path, t(page.label));
            navigate(page.path);
        },
        [setOpen, addRecentPage, navigate, t]
    );

    const handleClose = useCallback(() => setOpen(false), [setOpen]);

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            maxWidth="sm"
            fullWidth
            slotProps={{
                backdrop: { sx: { bgcolor: 'rgba(0,0,0,0.4)', backdropFilter: 'blur(2px)' } },
            }}
            PaperProps={{
                sx: {
                    borderRadius: '2px',
                    overflow: 'hidden',
                    maxHeight: '70vh',
                },
            }}
            TransitionProps={{
                onEntered: handleDialogEntered,
            }}
        >
            {/* Search Header */}
            <Box
                sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1.5,
                    px: 2.5,
                    py: 1.5,
                    borderBottom: '1px solid',
                    borderColor: 'divider',
                }}
            >
                <SearchIcon sx={{ color: 'text.secondary', fontSize: 22 }} />
                <InputBase
                    inputRef={inputRef}
                    placeholder={`${t('common.search')}...`}
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    onKeyDown={(e) => {
                        if (e.key === 'Escape') {
                            e.stopPropagation();
                            handleClose();
                        }
                    }}
                    sx={{ flex: 1, fontSize: '0.9375rem' }}
                />
                <Chip
                    label="esc"
                    size="small"
                    onClick={handleClose}
                    sx={{
                        height: 24,
                        fontSize: '0.6875rem',
                        fontWeight: 600,
                        bgcolor: 'action.hover',
                        cursor: 'pointer',
                    }}
                />
            </Box>

            {/* Results */}
            <DialogContent sx={{ p: 2.5, pt: 2 }}>
                {Object.keys(groupedPages).length === 0 ? (
                    <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{ textAlign: 'center', py: 4 }}
                    >
                        {t('common.no_results_found', { query })}
                    </Typography>
                ) : (
                    Object.entries(groupedPages).map(([category, pages]) => (
                        <Box key={category} sx={{ mb: 3, '&:last-child': { mb: 0 } }}>
                            <Typography
                                variant="overline"
                                sx={{
                                    fontSize: '0.6875rem',
                                    fontWeight: 700,
                                    color: 'text.secondary',
                                    letterSpacing: '0.08em',
                                    mb: 1.5,
                                    display: 'block',
                                }}
                            >
                                {t(`common.${category.toLowerCase()}`)}
                            </Typography>
                            <Grid container spacing={1.5}>
                                {pages.map((page) => (
                                    <Grid size={{ xs: 4, sm: 3 }} key={page.id}>
                                        <PageCard page={page} onClick={() => handleSelect(page)} />
                                    </Grid>
                                ))}
                            </Grid>
                        </Box>
                    ))
                )}
            </DialogContent>
        </Dialog>
    );
};

export default CommandPalette;
