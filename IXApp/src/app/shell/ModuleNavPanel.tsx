import React, { useState, useCallback, useEffect, useRef } from 'react';
import { COLORS, LAYOUT } from '@app/configuration/constants';
import { Box, Typography, Collapse, IconButton } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import ExpandAllIcon from '@mui/icons-material/AddBoxOutlined';
import CollapseAllIcon from '@mui/icons-material/IndeterminateCheckBoxOutlined';
import PeopleIcon from '@mui/icons-material/People';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import MoneyIcon from '@mui/icons-material/AttachMoney';
import BenefitsIcon from '@mui/icons-material/CardGiftcard';
import SelfServiceIcon from '@mui/icons-material/PersonOutlined';
import TaskIcon from '@mui/icons-material/Assignment';
import ProcessIcon from '@mui/icons-material/AccountTree';
import BackIcon from '@mui/icons-material/ArrowBackIosNew';
import CloseIcon from '@mui/icons-material/Close';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  getModuleNavLinkPermission,
  isRegisteredModuleNavLink,
  type ModuleNavSection,
} from '@app/configuration/navigation';
import { useAuth } from '@core/auth/useAuth';

const mobileOverlaySx = {
  position: 'absolute',
  top: 0,
  left: 0,
  right: 0,
  bottom: 0,
  zIndex: 10,
  bgcolor: 'background.paper',
} as const;

const desktopPanelBaseSx = {
  position: 'fixed',
  top: LAYOUT.TOPBARHEIGHT,
  borderRight: `1px solid ${COLORS.border}`,
  bottom: 0,
  width: LAYOUT.DRAWER_WIDTH,
  bgcolor: 'background.paper',
  boxShadow: '2px 0 8px rgba(0,0,0,0.1)',
  zIndex: (theme: { zIndex: { drawer: number } }) => theme.zIndex.drawer + 1,
  overflow: 'hidden',
  py: 1,
  transition: 'all 0.2s ease-in-out',
} as const;

const contentWrapperSx = {
  width: '100%',
  height: '100%',
  display: 'flex',
  flexDirection: 'column',
  bgcolor: 'background.paper',
} as const;

const mobileHeaderSx = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  bgcolor: COLORS.primary,
  color: 'white',
  px: 1,
  py: 0.5,
  minHeight: 52,
} as const;

const mobileHeaderLeftSx = {
  display: 'flex',
  alignItems: 'center',
} as const;

const mobileHeaderBackBtnSx = {
  color: 'white',
  mr: 1,
  transition: 'all 0.2s',
} as const;

const mobileHeaderCloseBtnSx = {
  color: 'white',
  transition: 'all 0.2s',
} as const;

const toolbarWrapperSx = {
  px: 2,
  py: 1.5,
  borderBottom: `1px solid ${COLORS.border}`,
} as const;

const toolbarRowSx = {
  display: 'flex',
  alignItems: 'center',
  gap: 2,
} as const;

const toolbarButtonSx = {
  display: 'flex',
  alignItems: 'center',
  gap: 0.5,
  cursor: 'pointer',
  transition: 'opacity 0.2s',
  opacity: 0.9,
  border: 0,
  p: 0,
  bgcolor: 'transparent',
  font: 'inherit',
  '&:hover': { opacity: 1 },
  '&:hover .toolbar-text': { textDecoration: 'underline' },
} as const;

const toolbarIconSx = {
  fontSize: 18,
  color: COLORS.primary,
} as const;

const toolbarLabelSx = {
  fontSize: '0.8125rem',
  color: COLORS.neutralText,
  fontWeight: 400,
} as const;

const sectionIconSx = {
  fontSize: 20,
  color: COLORS.neutralText,
  transition: 'transform 0.2s',
} as const;

const expandableChevronSx = {
  fontSize: 16,
  color: '#323130',
} as const;

const linkIconWrapperSx = {
  color: COLORS.primary,
  display: 'flex',
  transition: 'color 0.2s',
} as const;

const ICON_MAP: Record<string, React.ReactElement> = {
  people: <PeopleIcon sx={{ fontSize: 20 }} />,
  personAdd: <PersonAddIcon sx={{ fontSize: 20 }} />,
  money: <MoneyIcon sx={{ fontSize: 20 }} />,
  benefits: <BenefitsIcon sx={{ fontSize: 20 }} />,
  selfService: <SelfServiceIcon sx={{ fontSize: 20 }} />,
  task: <TaskIcon sx={{ fontSize: 20 }} />,
  process: <ProcessIcon sx={{ fontSize: 20 }} />,
};

const FALLBACK_ICON = <PeopleIcon sx={{ fontSize: 20 }} />;

const getSectionBorderSx = (isExpanded: boolean) => ({
  border: isExpanded ? `1px solid ${COLORS.primary}` : '1px solid transparent',
  borderRadius: '2px',
  overflow: 'hidden',
  transition: 'border-color 0.2s ease, background-color 0.2s ease',
});

const getSectionHeaderSx = (isExpanded: boolean, isMobileView: boolean) => ({
  width: '100%',
  border: 0,
  font: 'inherit',
  textAlign: 'start' as const,
  display: 'flex',
  alignItems: 'center',
  gap: 1,
  p: isMobileView ? 1.5 : 1,
  cursor: 'pointer',
  userSelect: 'none' as const,
  bgcolor: isExpanded ? COLORS.neutralSurface : 'transparent',
  transition: 'background-color 0.2s',
  '&:hover': { bgcolor: COLORS.neutralSurface },
});

const getSectionTitleSx = (isExpanded: boolean, isMobileView: boolean) => ({
  fontSize: isMobileView ? '0.9375rem' : '0.875rem',
  fontWeight: isExpanded ? 600 : 400,
  color: COLORS.neutralText,
  transition: 'font-weight 0.2s',
});

const getScrollableAreaSx = (isMobileView: boolean) => ({
  flex: 1,
  overflowY: 'auto' as const,
  overflowX: 'hidden' as const,
  p: 1,
  WebkitOverflowScrolling: 'touch' as const,
  '&::-webkit-scrollbar': { width: isMobileView ? '3px' : '6px' },
  '&::-webkit-scrollbar-track': { bgcolor: 'transparent' },
  '&::-webkit-scrollbar-thumb': { bgcolor: COLORS.neutralScrollbar, borderRadius: '3px' },
});

const getLinkContainerSx = (isMobileView: boolean) => ({
  p: 1,
  pl: isMobileView ? 5 : 4,
  bgcolor: 'background.paper',
});

const getExpandableLinkSx = (isMobileView: boolean) => ({
  display: 'flex',
  alignItems: 'center',
  gap: 0.5,
  cursor: 'pointer',
  py: isMobileView ? 1 : 0.5,
  transition: 'background-color 0.2s',
  borderRadius: '2px',
  px: 1,
  mx: -1,
  '&:hover': { bgcolor: COLORS.neutralSurface },
});

const getNavLinkSx = (isMobileView: boolean) => ({
  width: '100%',
  border: 0,
  bgcolor: 'transparent',
  font: 'inherit',
  textAlign: 'start' as const,
  display: 'flex',
  alignItems: 'center',
  cursor: 'pointer',
  py: isMobileView ? 1 : 0.6,
  transition: 'background-color 0.2s',
  borderRadius: '2px',
  px: 1,
  mx: -1,
  '&:hover': { bgcolor: COLORS.neutralSurface },
  '&:hover .link-text': { textDecoration: 'underline' },
});

const getLinkTextSx = (isMobileView: boolean) => ({
  fontSize: isMobileView ? '0.9375rem' : '0.875rem',
  color: COLORS.primary,
  textDecoration: 'none',
  transition: 'color 0.2s',
});

const getExpandableTitleSx = (isMobileView: boolean) => ({
  fontSize: isMobileView ? '0.9375rem' : '0.875rem',
  color: COLORS.neutralText,
  fontWeight: 600,
});

interface ModuleNavPanelProps {
  title: string;
  sections: ModuleNavSection[];
  onClose: () => void;
  onBack?: () => void;
  leftOffset: number;
  isMobileView?: boolean;
}

const ModuleNavPanel: React.FC<ModuleNavPanelProps> = ({
  title,
  sections,
  onClose,
  onBack,
  leftOffset,
  isMobileView,
}) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { hasPermission } = useAuth();

  const isLinkVisible = (link: ModuleNavSection['links'][number]) => {
    if (!isRegisteredModuleNavLink(link)) return false;
    const permission = getModuleNavLinkPermission(link);
    return !permission || hasPermission(permission);
  };

  const visibleSections = sections
    .map((section) => ({ ...section, links: section.links.filter(isLinkVisible) }))
    .filter((section) => section.links.length > 0);
  const panelRef = useRef<HTMLDivElement>(null);
  const allIds = sections.map((s) => s.id);
  const [expanded, setExpanded] = useState<Set<string>>(new Set());

  const toggle = useCallback((id: string) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const expandAll = useCallback(() => setExpanded(new Set(allIds)), [allIds]);
  const collapseAll = useCallback(() => setExpanded(new Set()), []);

  const handleLinkClick = useCallback(
    (path?: string) => {
      if (path) navigate(path);
      if (isMobileView) onClose();
    },
    [navigate, isMobileView, onClose]
  );

  // Close on click outside (only for desktop)
  useEffect(() => {
    if (isMobileView) return;
    const handler = (e: MouseEvent) => {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
        onClose();
      }
    };
    const timer = setTimeout(() => document.addEventListener('mousedown', handler), 0);
    return () => {
      clearTimeout(timer);
      document.removeEventListener('mousedown', handler);
    };
  }, [onClose, isMobileView]);

  // Close on Escape
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [onClose]);

  const renderContent = () => (
    <Box sx={contentWrapperSx}>
      {/* Mobile Header */}
      {isMobileView && (
        <Box sx={mobileHeaderSx}>
          <Box sx={mobileHeaderLeftSx}>
            <IconButton size="small" onClick={onBack || onClose} sx={mobileHeaderBackBtnSx}>
              <BackIcon sx={{ fontSize: 18 }} />
            </IconButton>
            <Typography sx={{ fontSize: '1rem', fontWeight: 600 }}>{t(title)}</Typography>
          </Box>
          <IconButton size="small" onClick={onClose} sx={mobileHeaderCloseBtnSx}>
            <CloseIcon sx={{ fontSize: 22 }} />
          </IconButton>
        </Box>
      )}

      {/* Toolbar */}
      <Box sx={toolbarWrapperSx}>
        <Box sx={toolbarRowSx}>
          <Box
            component="button"
            type="button"
            onClick={expandAll}
            sx={toolbarButtonSx}
            aria-label={t('common.expand_all', 'Expand All')}
          >
            <ExpandAllIcon sx={toolbarIconSx} />
            <Typography className="toolbar-text" sx={toolbarLabelSx}>
              {t('common.expand_all', 'Expand All')}
            </Typography>
          </Box>
          <Box
            component="button"
            type="button"
            onClick={collapseAll}
            sx={toolbarButtonSx}
            aria-label={t('common.collapse_all', 'Collapse All')}
          >
            <CollapseAllIcon sx={toolbarIconSx} />
            <Typography className="toolbar-text" sx={toolbarLabelSx}>
              {t('common.collapse_all', 'Collapse All')}
            </Typography>
          </Box>
        </Box>
      </Box>

      {/* Sections */}
      <Box sx={getScrollableAreaSx(!!isMobileView)}>
        {visibleSections.map((section) => {
          const isExpanded = expanded.has(section.id);
          return (
            <Box key={section.id} sx={{ mb: 0.5 }}>
              <Box sx={getSectionBorderSx(isExpanded)}>
                {/* Section header */}
                <Box
                  component="button"
                  type="button"
                  onClick={() => toggle(section.id)}
                  aria-expanded={isExpanded}
                  sx={getSectionHeaderSx(isExpanded, !!isMobileView)}
                >
                  {isExpanded ? (
                    <ExpandMoreIcon sx={sectionIconSx} />
                  ) : (
                    <ChevronRightIcon sx={sectionIconSx} />
                  )}
                  <Typography sx={getSectionTitleSx(isExpanded, !!isMobileView)}>
                    {t(section.title)}
                  </Typography>
                </Box>

                {/* Nested links under section */}
                <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                  <Box sx={getLinkContainerSx(!!isMobileView)}>
                    {section.links.map((link) =>
                      link.expandable ? (
                        <Box key={link.label} sx={getExpandableLinkSx(!!isMobileView)}>
                          <ChevronRightIcon sx={expandableChevronSx} />
                          <Typography sx={getExpandableTitleSx(!!isMobileView)}>
                            {t(link.label)}
                          </Typography>
                        </Box>
                      ) : (
                        <Box
                          key={link.label}
                          component={link.path ? 'button' : 'div'}
                          type={link.path ? 'button' : undefined}
                          onClick={() => handleLinkClick(link.path)}
                          sx={{
                            ...getNavLinkSx(!!isMobileView),
                            gap: link.icon ? 1.5 : 0,
                          }}
                        >
                          {link.icon && (
                            <Box sx={linkIconWrapperSx}>{ICON_MAP[link.icon] || FALLBACK_ICON}</Box>
                          )}
                          <Typography className="link-text" sx={getLinkTextSx(!!isMobileView)}>
                            {t(link.label)}
                          </Typography>
                        </Box>
                      )
                    )}
                  </Box>
                </Collapse>
              </Box>
            </Box>
          );
        })}
      </Box>
    </Box>
  );

  if (isMobileView) {
    return (
      <Box data-module-nav-panel="true" sx={mobileOverlaySx}>
        {renderContent()}
      </Box>
    );
  }

  // Default desktop rendering
  return (
    <Box
      ref={panelRef}
      data-module-nav-panel="true"
      sx={{ ...desktopPanelBaseSx, left: leftOffset }}
    >
      {renderContent()}
    </Box>
  );
};

export default React.memo(ModuleNavPanel);
