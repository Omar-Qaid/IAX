import React from 'react';
import {
    Box, Typography, IconButton, Drawer, CircularProgress, alpha
} from '@mui/material';
import { 
    Timeline, TimelineItem, TimelineSeparator, 
    TimelineDot, TimelineConnector, TimelineContent 
} from '@mui/lab';
import { 
    Close as CloseIcon, History as HistoryIcon, Undo as UndoIcon,
    HistoryEdu as HistoryEduIcon
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { format } from 'date-fns';

// ─── Sub-Components ─────────────────────────────────────────────────────────

const EmptyHistory: React.FC = () => {
    const { t } = useTranslation();
    return (
        <Box sx={{ 
            height: '100%', display: 'flex', flexDirection: 'column', 
            alignItems: 'center', justifyContent: 'center', p: 4, textAlign: 'center',
            opacity: 0.8
        }}>
            <Box sx={{ 
                width: 80, height: 80, borderRadius: '50%', 
                bgcolor: 'action.hover', display: 'flex', 
                alignItems: 'center', justifyContent: 'center', mb: 2
            }}>
                <HistoryEduIcon sx={{ fontSize: 40, color: 'text.disabled' }} />
            </Box>
            <Typography variant="subtitle1" fontWeight={700} color="text.secondary" gutterBottom>
                {t('common.no_history')}
            </Typography>
            <Typography variant="body2" color="text.disabled" sx={{ maxWidth: 200 }}>
                {t('common.no_history_msg')}
            </Typography>
        </Box>
    );
};

interface ChangeItemProps {
    field: string;
    oldValue: string;
    newValue: string;
}

const ChangeItem: React.FC<ChangeItemProps> = ({ field, oldValue, newValue }) => {
    const { t } = useTranslation();
    return (
        <Box sx={{ mb: 1, '&:last-child': { mb: 0 } }}>
            <Typography variant="caption" sx={{ display: 'block', color: 'text.secondary', fontWeight: 600 }}>
                {t(field)}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                <Typography variant="caption" sx={{ 
                    textDecoration: oldValue !== 'NULL' ? 'line-through' : 'none', 
                    color: 'error.main', 
                    bgcolor: (theme) => alpha(theme.palette.error.main, 0.1), 
                    px: 0.5, borderRadius: 0.5
                }}>
                    {oldValue}
                </Typography>
                <Typography variant="caption" color="text.secondary">→</Typography>
                <Typography variant="caption" sx={{ 
                    color: 'success.main', 
                    bgcolor: (theme) => alpha(theme.palette.success.main, 0.1), 
                    px: 0.5, borderRadius: 0.5, fontWeight: 600
                }}>
                    {newValue}
                </Typography>
            </Box>
        </Box>
    );
};

interface AuditTimelineItemProps {
    date: string;
    user: string;
    action: string;
    color: any;
    icon: React.ReactNode;
    changes: any[];
    isLast: boolean;
}

const AuditTimelineItem: React.FC<AuditTimelineItemProps> = ({ 
    date, user, action, color, icon, changes, isLast 
}) => {
    const { t } = useTranslation();
    return (
        <TimelineItem>
            <TimelineSeparator>
                <TimelineDot color={color} sx={{ boxShadow: 'none' }}>
                    {icon}
                </TimelineDot>
                {!isLast && <TimelineConnector />}
            </TimelineSeparator>
            <TimelineContent sx={{ py: '12px', px: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 0.5 }}>
                    <Typography variant="subtitle2" fontWeight={600} sx={{ lineHeight: 1.2 }}>
                        {t(`common.${action.toLowerCase()}`)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary" sx={{ whiteSpace: 'nowrap' }}>
                        {format(new Date(date), 'MMM dd, HH:mm')}
                    </Typography>
                </Box>
                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.8rem', mb: 1 }}>
                    {t('common.by')} <Box component="span" sx={{ color: 'text.primary', fontWeight: 500 }}>{user}</Box>
                </Typography>
                
                {changes && changes.length > 0 && (
                    <Box sx={{ 
                        p: 1.5, bgcolor: 'action.hover', 
                        borderRadius: 2, border: '1px solid', borderColor: 'divider'
                    }}>
                        {changes.map((change, idx) => (
                            <ChangeItem 
                                key={idx} 
                                field={change.field} 
                                oldValue={change.old} 
                                newValue={change.new} 
                            />
                        ))}
                    </Box>
                )}
            </TimelineContent>
        </TimelineItem>
    );
};

// ─── Main Component ──────────────────────────────────────────────────────────

export interface HistoryDrawerProps {
    open: boolean;
    onClose: () => void;
    history: any[];
    loading: boolean;
    page: number;
    hasMore: boolean;
    observerTarget: React.RefObject<HTMLDivElement | null>;
    title?: string;
    onRefresh?: () => void;
}

export const HistoryDrawer: React.FC<HistoryDrawerProps> = ({
    open, onClose, history, loading, page, observerTarget, title, onRefresh
}) => {
    const { t } = useTranslation();
    const displayTitle = title ? t(title) : t('common.audit_history');
    return (
        <Drawer
            anchor="right"
            open={open}
            onClose={onClose}
            PaperProps={{
                sx: { width: { xs: '100%', sm: 400 }, borderLeft: 'none' }
            }}
            sx={{ zIndex: (theme) => theme.zIndex.drawer + 2 }}
        >
            <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                <Box sx={{ 
                    p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                    borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'action.hover'
                }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <HistoryIcon color="primary" />
                        <Typography variant="subtitle1" fontWeight={700}>{displayTitle}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        <IconButton onClick={onRefresh} size="small" disabled={loading}>
                            <UndoIcon fontSize="small" sx={{ 
                                animation: loading ? 'spin 1s linear infinite' : 'none',
                                '@keyframes spin': {
                                    '0%': { transform: 'rotate(0deg)' },
                                    '100%': { transform: 'rotate(360deg)' }
                                }
                            }} />
                        </IconButton>
                        <IconButton onClick={onClose} size="small"><CloseIcon fontSize="small" /></IconButton>
                    </Box>
                </Box>

                <Box sx={{ flexGrow: 1, overflowY: 'auto', p: 1 }}>
                    {loading && page === 1 ? (
                        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}><CircularProgress size={24} /></Box>
                    ) : history.length === 0 && !loading ? (
                        <EmptyHistory />
                    ) : (
                        <>
                            <Timeline position="right" sx={{ 
                                p: 0, [`& .MuiTimelineItem-root:before`]: { flex: 0, padding: 0 } 
                            }}>
                                {history.map((item, index) => (
                                    <AuditTimelineItem 
                                        key={index}
                                        {...item}
                                        isLast={index === history.length - 1}
                                    />
                                ))}
                            </Timeline>
                            <div ref={observerTarget} style={{ height: 20, marginBottom: 20 }} />
                            {loading && page > 1 && (
                                <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}><CircularProgress size={20} /></Box>
                            )}
                        </>
                    )}
                </Box>

                <Box sx={{ py: 0.75, px: 2, borderTop: '1px solid', borderColor: 'divider', bgcolor: 'background.paper' }}>
                    <Typography variant="caption" color="text.secondary">
                        {t('common.total_activities', { count: history.length })}
                    </Typography>
                </Box>
            </Box>
        </Drawer>
    );
};
