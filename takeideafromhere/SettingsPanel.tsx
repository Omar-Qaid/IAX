import React from 'react';
import {
    Drawer,
    Box,
    Typography,
    IconButton,
    Switch,
    Slider,
    Tooltip,
    Divider,
    Button,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import RefreshIcon from '@mui/icons-material/Refresh';
import SettingsIcon from '@mui/icons-material/Settings';
import DarkModeIcon from '@mui/icons-material/DarkMode';
import ContrastIcon from '@mui/icons-material/Contrast';
import FormatTextdirectionRToLIcon from '@mui/icons-material/FormatTextdirectionRToL';
import CompressIcon from '@mui/icons-material/Compress';
import ViewSidebarIcon from '@mui/icons-material/ViewSidebar';
import ViewStreamIcon from '@mui/icons-material/ViewStream';
import ViewCompactIcon from '@mui/icons-material/ViewCompact';
import { useTranslation } from 'react-i18next';
import { useUIStore } from '../../store/uiStore';
import { COLOR_PRESETS, FONT_OPTIONS } from '../../theme/createAppTheme';

const DRAWER_WIDTH = 340;

// ─── Toggle Card ──────────────────────────────────────────────────────────
const ToggleCard: React.FC<{
    icon: React.ReactNode;
    label: string;
    checked: boolean;
    onChange: (v: boolean) => void;
}> = ({ icon, label, checked, onChange }) => (
    <Box
        onClick={() => onChange(!checked)}
        sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 0.75,
            p: 1.5,
            borderRadius: '2px',
            border: '1px solid',
            borderColor: checked ? 'primary.main' : '#e2e8f0',
            bgcolor: checked ? 'rgba(99,102,241,0.04)' : '#fff',
            cursor: 'pointer',
            transition: 'all 0.2s',
            flex: 1,
            '&:hover': { borderColor: checked ? 'primary.main' : '#cbd5e1' },
        }}
    >
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
            <Box sx={{ color: checked ? 'primary.main' : '#94a3b8', fontSize: 20, display: 'flex' }}>{icon}</Box>
            <Switch
                checked={checked}
                size="small"
                onClick={(e) => e.stopPropagation()}
                onChange={(_, v) => onChange(v)}
                sx={{
                    width: 36, height: 20, p: 0,
                    '& .MuiSwitch-switchBase': {
                        p: '3px',
                        '&.Mui-checked': { transform: 'translateX(16px)', color: '#fff', '& + .MuiSwitch-track': { bgcolor: 'primary.main', opacity: 1 } },
                    },
                    '& .MuiSwitch-thumb': { width: 14, height: 14 },
                    '& .MuiSwitch-track': { borderRadius: 10, bgcolor: '#e2e8f0', opacity: 1 },
                }}
            />
        </Box>
        <Typography sx={{ fontSize: '0.75rem', color: '#64748b', fontWeight: 500 }}>{label}</Typography>
    </Box>
);

// ─── Section Label ────────────────────────────────────────────────────────
const SectionLabel: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <Typography sx={{
        fontSize: '0.6875rem',
        fontWeight: 700,
        color: '#1e293b',
        textTransform: 'uppercase',
        letterSpacing: '0.08em',
        mb: 1.5,
        px: 0.5,
        display: 'inline-block',
        bgcolor: '#e2e8f0',
        borderRadius: '2px',
        py: 0.25,
    }}>
        {children}
    </Typography>
);

// ─── Layout Option ────────────────────────────────────────────────────────
const LayoutOption: React.FC<{
    icon: React.ReactNode;
    selected: boolean;
    onClick: () => void;
}> = ({ icon, selected, onClick }) => (
    <Box
        onClick={onClick}
        sx={{
            width: 56,
            height: 48,
            borderRadius: '2px',
            border: '2px solid',
            borderColor: selected ? 'primary.main' : '#e2e8f0',
            bgcolor: selected ? 'rgba(99,102,241,0.06)' : '#fff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            cursor: 'pointer',
            transition: 'all 0.2s',
            color: selected ? 'primary.main' : '#94a3b8',
            '&:hover': { borderColor: selected ? 'primary.main' : '#cbd5e1' },
        }}
    >
        {icon}
    </Box>
);

// ─── Color Option ─────────────────────────────────────────────────────────
const ColorOption: React.FC<{
    label: string;
    selected: boolean;
    icon: React.ReactNode;
    onClick: () => void;
}> = ({ label, selected, icon, onClick }) => (
    <Box
        onClick={onClick}
        sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1,
            px: 2,
            py: 1,
            borderRadius: '2px',
            border: '1px solid',
            borderColor: selected ? 'primary.main' : '#e2e8f0',
            bgcolor: selected ? 'rgba(99,102,241,0.04)' : '#fff',
            cursor: 'pointer',
            flex: 1,
            transition: 'all 0.2s',
            '&:hover': { borderColor: selected ? 'primary.main' : '#cbd5e1' },
        }}
    >
        {icon}
        <Typography sx={{ fontSize: '0.75rem', fontWeight: 500, color: '#475569' }}>{label}</Typography>
    </Box>
);

// ─── Preset Circle ────────────────────────────────────────────────────────
const PresetCircle: React.FC<{
    color: string;
    selected: boolean;
    onClick: () => void;
}> = ({ color, selected, onClick }) => (
    <Box
        onClick={onClick}
        sx={{
            width: 44,
            height: 44,
            borderRadius: '2px',
            bgcolor: '#f8fafc',
            border: '2px solid',
            borderColor: selected ? color : '#e2e8f0',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            cursor: 'pointer',
            transition: 'all 0.2s',
            '&:hover': { borderColor: color },
        }}
    >
        <Box sx={{
            width: 24,
            height: 24,
            borderRadius: '50%',
            bgcolor: color,
        }} />
    </Box>
);

// ─── Font Card ────────────────────────────────────────────────────────────
const FontCard: React.FC<{
    label: string;
    fontFamily: string;
    selected: boolean;
    onClick: () => void;
}> = ({ label, fontFamily, selected, onClick }) => (
    <Box
        onClick={onClick}
        sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 0.5,
            p: 1.5,
            borderRadius: '2px',
            border: '2px solid',
            borderColor: selected ? 'primary.main' : '#e2e8f0',
            bgcolor: selected ? 'rgba(99,102,241,0.04)' : '#fff',
            cursor: 'pointer',
            flex: 1,
            transition: 'all 0.2s',
            '&:hover': { borderColor: selected ? 'primary.main' : '#cbd5e1' },
        }}
    >
        <Typography sx={{ fontSize: '1.25rem', fontFamily, fontWeight: 600, color: '#475569' }}>
            Aa
        </Typography>
        <Typography sx={{ fontSize: '0.6875rem', color: '#94a3b8', fontWeight: 500 }}>
            {label}
        </Typography>
    </Box>
);

// ─── Settings Panel ───────────────────────────────────────────────────────
const SettingsPanel: React.FC = () => {
    const { t } = useTranslation();
    const settingsPanelOpen = useUIStore((s) => s.settingsPanelOpen);
    const setSettingsPanelOpen = useUIStore((s) => s.setSettingsPanelOpen);
    const colorMode = useUIStore((s) => s.colorMode);
    const toggleColorMode = useUIStore((s) => s.toggleColorMode);
    const contrast = useUIStore((s) => s.contrast);
    const setContrast = useUIStore((s) => s.setContrast);
    const rtl = useUIStore((s) => s.rtl);
    const setRtl = useUIStore((s) => s.setRtl);
    const compact = useUIStore((s) => s.compact);
    const setCompact = useUIStore((s) => s.setCompact);
    const navLayout = useUIStore((s) => s.navLayout);
    const setNavLayout = useUIStore((s) => s.setNavLayout);
    const navColor = useUIStore((s) => s.navColor);
    const setNavColor = useUIStore((s) => s.setNavColor);
    const colorPreset = useUIStore((s) => s.colorPreset);
    const setColorPreset = useUIStore((s) => s.setColorPreset);
    const fontFamily = useUIStore((s) => s.fontFamily);
    const setFontFamily = useUIStore((s) => s.setFontFamily);
    const fontSize = useUIStore((s) => s.fontSize);
    const setFontSize = useUIStore((s) => s.setFontSize);
    const zoom = useUIStore((s) => s.zoom);
    const setZoom = useUIStore((s) => s.setZoom);
    const resetSettings = useUIStore((s) => s.resetSettings);
    const clearRecentPages = useUIStore((s) => s.clearRecentPages);

    const handleClose = () => setSettingsPanelOpen(false);

    return (
        <Drawer
            anchor="right"
            open={settingsPanelOpen}
            onClose={handleClose}
            PaperProps={{
                sx: {
                    width: { xs: '100vw', sm: DRAWER_WIDTH },
                    maxWidth: '100vw',
                    borderRadius: 0,
                    boxShadow: '-4px 0 24px rgba(0,0,0,0.08)',
                    bgcolor: '#fff',
                    pt: '40px',
                },
            }}
        >
            {/* Header */}
            <Box sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                px: 2.5,
                pt: 2.5,
                pb: 1.5,
            }}>
                <Typography sx={{ fontSize: '1.125rem', fontWeight: 700, color: '#1e293b' }}>
                    {t('settings.title')}
                </Typography>
                <Box sx={{ display: 'flex', gap: 0.25 }}>
                    <Tooltip title={t('settings.title')}>
                        <IconButton size="small" sx={{ color: '#94a3b8' }}>
                            <SettingsIcon sx={{ fontSize: 18 }} />
                        </IconButton>
                    </Tooltip>
                    <Tooltip title={t('settings.reset')}>
                        <IconButton size="small" onClick={resetSettings} sx={{ color: '#94a3b8' }}>
                            <RefreshIcon sx={{ fontSize: 18 }} />
                        </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.close')}>
                        <IconButton size="small" onClick={handleClose} sx={{ color: '#94a3b8' }}>
                            <CloseIcon sx={{ fontSize: 18 }} />
                        </IconButton>
                    </Tooltip>
                </Box>
            </Box>

            {/* Scrollable content */}
            <Box sx={{
                flex: 1,
                overflowY: 'auto',
                px: 2.5,
                pb: 3,
                '&::-webkit-scrollbar': { width: '4px' },
                '&::-webkit-scrollbar-thumb': { bgcolor: '#cbd5e1', borderRadius: '2px' },
            }}>
                {/* Toggle Cards */}
                <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.5, mb: 3 }}>
                    <ToggleCard
                        icon={<DarkModeIcon sx={{ fontSize: 20 }} />}
                        label={t('settings.mode')}
                        checked={colorMode === 'dark'}
                        onChange={() => toggleColorMode()}
                    />
                    <ToggleCard
                        icon={<ContrastIcon sx={{ fontSize: 20 }} />}
                        label={t('settings.contrast')}
                        checked={contrast}
                        onChange={setContrast}
                    />
                    <ToggleCard
                        icon={<FormatTextdirectionRToLIcon sx={{ fontSize: 20 }} />}
                        label={t('settings.rtl')}
                        checked={rtl}
                        onChange={setRtl}
                    />
                    <ToggleCard
                        icon={<CompressIcon sx={{ fontSize: 20 }} />}
                        label={t('settings.compact')}
                        checked={compact}
                        onChange={setCompact}
                    />
                </Box>

                <Divider sx={{ mb: 2.5 }} />

                {/* Nav Section */}
                <SectionLabel>{t('settings.nav')}</SectionLabel>

                {/* Layout */}
                <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', mb: 1 }}>{t('settings.layout')}</Typography>
                <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
                    <LayoutOption
                        icon={<ViewSidebarIcon sx={{ fontSize: 22 }} />}
                        selected={navLayout === 'vertical'}
                        onClick={() => setNavLayout('vertical')}
                    />
                    <LayoutOption
                        icon={<ViewStreamIcon sx={{ fontSize: 22 }} />}
                        selected={navLayout === 'horizontal'}
                        onClick={() => setNavLayout('horizontal')}
                    />
                    <LayoutOption
                        icon={<ViewCompactIcon sx={{ fontSize: 22 }} />}
                        selected={navLayout === 'mini'}
                        onClick={() => setNavLayout('mini')}
                    />
                </Box>

                {/* Color */}
                <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', mb: 1 }}>{t('settings.color')}</Typography>
                <Box sx={{ display: 'flex', gap: 1, mb: 3 }}>
                    <ColorOption
                        label={t('settings.integrate')}
                        selected={navColor === 'integrate'}
                        icon={<Box sx={{ width: 20, height: 20, borderRadius: '2px', bgcolor: 'primary.main', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><ViewSidebarIcon sx={{ fontSize: 12, color: '#fff' }} /></Box>}
                        onClick={() => setNavColor('integrate')}
                    />
                    <ColorOption
                        label={t('settings.apparent')}
                        selected={navColor === 'apparent'}
                        icon={<Box sx={{ width: 20, height: 20, borderRadius: '2px', bgcolor: '#1e293b', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><ViewSidebarIcon sx={{ fontSize: 12, color: '#fff' }} /></Box>}
                        onClick={() => setNavColor('apparent')}
                    />
                </Box>

                <Divider sx={{ mb: 2.5 }} />

                {/* Presets */}
                <SectionLabel>{t('settings.presets')}</SectionLabel>
                <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 1.5, mb: 3 }}>
                    {Object.entries(COLOR_PRESETS).map(([key, preset]) => (
                        <PresetCircle
                            key={key}
                            color={preset.primary.main}
                            selected={colorPreset === key}
                            onClick={() => setColorPreset(key)}
                        />
                    ))}
                </Box>

                <Divider sx={{ mb: 2.5 }} />

                {/* Font Section */}
                <SectionLabel>{t('settings.font')}</SectionLabel>

                <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', mb: 1 }}>{t('settings.family')}</Typography>
                <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1, mb: 2.5 }}>
                    {FONT_OPTIONS.map((f) => (
                        <FontCard
                            key={f.label}
                            label={f.label}
                            fontFamily={f.value}
                            selected={fontFamily === f.label}
                            onClick={() => setFontFamily(f.label)}
                        />
                    ))}
                </Box>

                <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', mb: 1 }}>{t('settings.size')}</Typography>
                <Box sx={{ px: 1 }}>
                    <Slider
                        value={fontSize}
                        onChange={(_, v) => setFontSize(v as number)}
                        min={12}
                        max={20}
                        step={1}
                        valueLabelDisplay="on"
                        valueLabelFormat={(v) => `${v}px`}
                        sx={{
                            color: 'primary.main',
                            height: 4,
                            '& .MuiSlider-thumb': {
                                width: 20,
                                height: 20,
                                bgcolor: '#fff',
                                border: '2px solid currentColor',
                                '&:hover': { boxShadow: '0 0 0 6px rgba(99,102,241,0.16)' },
                            },
                            '& .MuiSlider-valueLabel': {
                                bgcolor: '#1e293b',
                                borderRadius: '2px',
                                fontSize: '0.6875rem',
                                fontWeight: 600,
                            },
                            '& .MuiSlider-track': { borderRadius: 2 },
                            '& .MuiSlider-rail': { bgcolor: '#e2e8f0' },
                        }}
                    />
                </Box>

                <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', mt: 2.5, mb: 1 }}>{t('settings.zoom', 'Desktop Zoom')}</Typography>
                <Box sx={{ px: 1 }}>
                    <Slider
                        value={zoom}
                        onChange={(_, v) => setZoom(v as number)}
                        min={50}
                        max={150}
                        step={5}
                        valueLabelDisplay="on"
                        valueLabelFormat={(v) => `${v}%`}
                        sx={{
                            color: 'primary.main',
                            height: 4,
                            '& .MuiSlider-thumb': {
                                width: 20,
                                height: 20,
                                bgcolor: '#fff',
                                border: '2px solid currentColor',
                                '&:hover': { boxShadow: '0 0 0 6px rgba(99,102,241,0.16)' },
                            },
                            '& .MuiSlider-valueLabel': {
                                bgcolor: '#1e293b',
                                borderRadius: '2px',
                                fontSize: '0.6875rem',
                                fontWeight: 600,
                            },
                            '& .MuiSlider-track': { borderRadius: 2 },
                            '& .MuiSlider-rail': { bgcolor: '#e2e8f0' },
                        }}
                    />
                </Box>

                <Divider sx={{ my: 2.5 }} />

                {/* Data Section */}
                <SectionLabel>{t('settings.data')}</SectionLabel>
                <Box sx={{ pb: 2 }}>
                    <Button
                        variant="outlined"
                        color="error"
                        fullWidth
                        onClick={clearRecentPages}
                        sx={{
                            borderRadius: '2px',
                            textTransform: 'none',
                            fontWeight: 600,
                        }}
                    >
                        {t('settings.clear_history')}
                    </Button>
                </Box>

            </Box>
        </Drawer>
    );
};

export default SettingsPanel;

