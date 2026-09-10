import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { notificationInboxApi } from './notificationInboxApi';
import {
    Drawer,
    Box,
    Typography,
    IconButton,
    Tabs,
    Tab,
    Avatar,
    Button,
    Divider,
    Tooltip,
} from '@mui/material';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import SettingsIcon from '@mui/icons-material/Settings';
import CloseIcon from '@mui/icons-material/Close';
import InsertDriveFileIcon from '@mui/icons-material/InsertDriveFile';
import ArchiveIcon from '@mui/icons-material/ArchiveOutlined';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import { useTranslation } from 'react-i18next';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { LAYOUT } from '@app/configuration/constants';
import { useLogicalDrawerAnchor } from '@shared/hooks/useLogicalDrawerAnchor';

const DRAWER_WIDTH = 380;

// ─── Types & Mocks ──────────────────────────────────────────────────────────

export interface AppDrawerNotification {
    id: string;
    sender?: string;
    avatar?: string;
    message: string;
    createdAt: string;
    read: boolean;
    archived: boolean;
    priority?: 'Critical' | 'High' | 'Medium' | 'Low';
    category?: string;
    actions?: { label: string; variant: 'contained' | 'outlined' }[];
    attachment?: { name: string; size: string };
}

function timeAgo(dateStr: string, language: string): string {
    const now = Date.now();
    const timestamp = new Date(dateStr).getTime();
    if (!Number.isFinite(timestamp)) return '';
    const diff = Math.max(0, now - timestamp);
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    const relative = new Intl.RelativeTimeFormat(language, { numeric: 'auto' });
    if (seconds < 60) return relative.format(-Math.max(1, seconds), 'second');
    if (minutes < 60) return relative.format(-minutes, 'minute');
    if (hours < 24) return relative.format(-hours, 'hour');
    return relative.format(-days, 'day');
}

// ─── Notification Item ───────────────────────────────────────────────────────

const NotificationItem: React.FC<{
    notification: AppDrawerNotification;
    onMarkRead: (id: string) => void;
    onArchive: (id: string) => void;
    onDelete: (id: string) => void;
}> = ({ notification, onMarkRead, onArchive, onDelete }) => {
    const { t, i18n } = useTranslation();
    const n = notification;

    const getPriorityColor = (priority?: string) => {
        switch (priority) {
            case 'Critical': return '#ef4444';
            case 'High': return '#f97316';
            case 'Medium': return '#3b82f6';
            case 'Low': default: return '#10b981';
        }
    };

    return (
        <Box
            onClick={() => !n.read && onMarkRead(n.id)}
            sx={{
                display: 'flex',
                gap: 1.5,
                px: 2.5,
                py: 2,
                cursor: n.read ? 'default' : 'pointer',
                transition: 'background-color 0.15s',
                '&:hover': { 
                    bgcolor: 'action.hover',
                    '& .hover-actions': { opacity: 1 }
                },
                position: 'relative',
            }}
        >
            <Box sx={{
                position: 'absolute', insetInlineStart: 0, top: 0, bottom: 0, width: 3,
                bgcolor: getPriorityColor(n.priority),
            }} />

            <Avatar src={n.avatar} sx={{ width: 40, height: 40, flexShrink: 0, mt: 0.25 }}>
                {n.sender?.[0] || '?'}
            </Avatar>

            <Box sx={{ flex: 1, minWidth: 0, paddingInlineEnd: 4 }}>
                <Typography sx={{ fontSize: '0.8125rem', color: 'text.primary', lineHeight: 1.5 }}>
                    <strong>{n.sender ?? ''}</strong> {n.message}
                </Typography>
                <Typography sx={{ fontSize: '0.75rem', color: 'text.secondary', mt: 0.25 }}>
                    {timeAgo(n.createdAt, i18n.resolvedLanguage ?? i18n.language)}
                    {n.category && ` \u00B7 ${n.category}`}
                    {n.priority && ` \u00B7 ${t(`notifications.priority.${n.priority}`)}`}
                </Typography>

                {n.actions && n.actions.length > 0 && (
                    <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
                        {n.actions.map((action) => (
                            <Button
                                key={action.label}
                                variant={action.variant}
                                size="small"
                                sx={{
                                    textTransform: 'none',
                                    fontSize: '0.75rem',
                                    fontWeight: 600,
                                    borderRadius: '2px',
                                    px: 1.5,
                                    py: 0.25,
                                    minHeight: 28,
                                }}
                            >
                                {t(action.label)}
                            </Button>
                        ))}
                    </Box>
                )}

                {n.attachment && (
                    <Box sx={{
                        display: 'flex', alignItems: 'center', gap: 1, mt: 1, p: 1,
                        border: '1px solid', borderColor: 'divider', borderRadius: '2px', bgcolor: 'background.paper',
                    }}>
                        <Box sx={{
                            width: 36, height: 36, borderRadius: '2px', bgcolor: '#ef4444',
                            display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0,
                        }}>
                            <InsertDriveFileIcon sx={{ fontSize: 18, color: '#fff' }} />
                        </Box>
                        <Box sx={{ flex: 1, minWidth: 0 }}>
                            <Typography sx={{ fontSize: '0.75rem', color: 'text.primary', fontWeight: 500, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                {n.attachment.name}
                            </Typography>
                            <Typography sx={{ fontSize: '0.6875rem', color: 'text.secondary' }}>
                                {n.attachment.size}
                            </Typography>
                        </Box>
                        <Button
                            variant="outlined"
                            size="small"
                            sx={{
                                textTransform: 'none', fontSize: '0.75rem', fontWeight: 600,
                                borderRadius: '2px', minHeight: 28, px: 1.5, flexShrink: 0,
                            }}
                        >
                            {t('actions.download')}
                        </Button>
                    </Box>
                )}
            </Box>

            {!n.read && (
                <Box sx={{
                    width: 8, height: 8, borderRadius: '50%', bgcolor: 'primary.main', flexShrink: 0, mt: 0.75,
                }} />
            )}

            <Box
                className="hover-actions"
                onClick={(e) => e.stopPropagation()}
                sx={{
                    position: 'absolute', top: 8, insetInlineEnd: 8, display: 'flex', gap: 0.5,
                    opacity: 0, transition: 'opacity 0.15s', bgcolor: 'background.paper',
                    borderRadius: 1, p: 0.25, boxShadow: '0 2px 4px rgba(0,0,0,0.05)',
                }}
            >
                {!n.archived && (
                    <Tooltip title={t('actions.archive')}>
                        <IconButton size="small" aria-label={t('actions.archive')} onClick={(event) => { event.stopPropagation(); onArchive(n.id); }} sx={{ p: 0.5 }}>
                            <ArchiveIcon sx={{ fontSize: 16 }} />
                        </IconButton>
                    </Tooltip>
                )}
                <Tooltip title={t('actions.delete')}>
                    <IconButton size="small" aria-label={t('actions.delete')} onClick={(event) => { event.stopPropagation(); onDelete(n.id); }} sx={{ p: 0.5 }}>
                        <DeleteIcon sx={{ fontSize: 16 }} />
                    </IconButton>
                </Tooltip>
            </Box>
        </Box>
    );
};

// ─── Notification Drawer ──────────────────────────────────────────────────────

export const AppNotificationDrawer: React.FC = () => {
    const { t } = useTranslation();
    const notificationDrawerOpen = useNavigationStore((s) => s.notificationDrawerOpen);
    const setNotificationDrawerOpen = useNavigationStore((s) => s.setNotificationDrawerOpen);
    
    const [tab, setTab] = useState(0);
    const [page, setPage] = useState(1);
    const { user } = useAuth();
    const company = useCompanyStore((state) => state.currentCompany);
    const client = useQueryClient();
    const key = ['notification-inbox', user?.id, company];
    const inbox = useQuery({
        queryKey: [...key, tab, page],
        queryFn: ({ signal }) => notificationInboxApi.list(page, tab, signal),
        enabled: notificationDrawerOpen && !!user,
        refetchInterval: notificationDrawerOpen ? 30000 : false,
    });
    const unread = useQuery({
        queryKey: [...key, 'unread'],
        queryFn: ({ signal }) => notificationInboxApi.unread(signal),
        enabled: notificationDrawerOpen && !!user,
        refetchInterval: notificationDrawerOpen ? 30000 : false,
    });
    const mutation = useMutation({
        mutationFn: ({ action, id }: { action: 'read' | 'archive' | 'delete' | 'read-all'; id?: string }) => notificationInboxApi.update(action, id),
        onSuccess: () => client.invalidateQueries({ queryKey: key }),
    });
    const filteredNotifications: AppDrawerNotification[] = (inbox.data?.items ?? []).map((item) => ({
        id: String(item.recId), sender: item.title, message: item.message, createdAt: item.createdAt ?? '',
        read: item.isRead, archived: item.isArchived, priority: item.priority, category: item.category,
    }));
    const unreadCount = unread.data ?? 0;
    const handleClose = () => setNotificationDrawerOpen(false);
    const drawerAnchor = useLogicalDrawerAnchor('end');
    const update = (action: 'read' | 'archive' | 'delete' | 'read-all', id?: string) => {
        if (!mutation.isPending) mutation.mutate({ action, id });
    };
    const markRead = (id: string) => update('read', id);
    const markAllRead = () => update('read-all');
    const archiveNotification = (id: string) => update('archive', id);
    const removeNotification = (id: string) => update('delete', id);

    return (
        <Drawer
            anchor={drawerAnchor}
            open={notificationDrawerOpen}
            onClose={handleClose}
            slotProps={{
                backdrop: {
                    sx: { top: `${LAYOUT.TOPBARHEIGHT}px` },
                },
                paper: {
                    sx: {
                        width: { xs: '100vw', sm: DRAWER_WIDTH },
                        maxWidth: '100vw',
                        top: `${LAYOUT.TOPBARHEIGHT}px`,
                        height: `calc(100% - ${LAYOUT.TOPBARHEIGHT}px)`,
                        borderRadius: 0,
                        boxShadow: '-4px 0 24px rgba(0,0,0,0.08)',
                        borderInlineStart: '1px solid',
                        borderInlineStartColor: 'divider',
                        borderInlineEnd: 0,
                        overflow: 'hidden',
                    },
                },
            }}
        >
            <Box sx={{
                display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                px: 2.5, minHeight: 56, py: 1, flexShrink: 0,
                borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'background.paper', zIndex: 1,
            }}>
                <Typography sx={{ fontSize: '1.125rem', fontWeight: 700, color: 'text.primary' }}>
                    {t('common.notifications', 'Notifications')}
                </Typography>
                <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Tooltip title={t('notifications.mark_all_read', 'Mark all as read')}>
                        <IconButton size="small" onClick={markAllRead} sx={{ color: 'text.secondary' }}>
                            <DoneAllIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.settings', 'Settings')}>
                        <IconButton size="small" sx={{ color: 'text.secondary' }}>
                            <SettingsIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.close', 'Close')}>
                        <IconButton size="small" onClick={handleClose} sx={{ color: 'text.secondary' }}>
                            <CloseIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>
                </Box>
            </Box>

            <Box sx={{ px: 2.5 }}>
                <Tabs
                    value={tab}
                    onChange={(_, v) => { setTab(v); setPage(1); }}
                    sx={{
                        minHeight: 36,
                        '& .MuiTabs-indicator': { display: 'none' },
                        '& .MuiTab-root': {
                            textTransform: 'none', minHeight: 32, fontSize: '0.8125rem', fontWeight: 600,
                            color: 'text.secondary', px: 1.5, py: 0.5, marginInlineEnd: 0.5, minWidth: 'auto',
                            '&.Mui-selected': { color: 'text.primary' },
                        },
                    }}
                >
                    <Tab label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                            {t('notifications.all', 'All')}
                            
                        </Box>
                    } />
                    <Tab label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                            {t('notifications.unread', 'Unread')}
                            <Box sx={{
                                bgcolor: tab === 1 ? 'text.primary' : 'action.selected',
                                color: tab === 1 ? 'background.paper' : 'text.secondary',
                                fontSize: '0.6875rem', fontWeight: 700, borderRadius: '2px',
                                minWidth: 20, height: 20, display: 'flex', alignItems: 'center', justifyContent: 'center', px: 0.5,
                            }}>
                                {unreadCount}
                            </Box>
                        </Box>
                    } />
                    <Tab label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                            {t('notifications.archived', 'Archived')}
                            
                        </Box>
                    } />
                </Tabs>
            </Box>

            <Divider sx={{ mt: 1 }} />

            <Box sx={{
                flex: 1, overflowY: 'auto',
                '&::-webkit-scrollbar': { width: '4px' },
                '&::-webkit-scrollbar-thumb': { bgcolor: 'divider', borderRadius: '2px' },
            }}>
                {(inbox.isError || mutation.isError) && <Typography color="error" sx={{ p: 2 }}>{t('notifications.loadError', 'Unable to load or update notifications. Please try again.')}</Typography>}
                {inbox.isLoading && <Typography sx={{ p: 2 }}>{t('common.loading', 'Loading...')}</Typography>}
                {filteredNotifications.length === 0 ? (
                    <Box sx={{ py: 6, textAlign: 'center' }}>
                        <Typography sx={{ fontSize: '0.875rem', color: 'text.secondary' }}>
                            {t('notifications.no_notifications', 'No notifications to display')}
                        </Typography>
                    </Box>
                ) : (
                    filteredNotifications.map((n, index) => (
                        <React.Fragment key={n.id}>
                            <NotificationItem 
                                notification={n} 
                                onMarkRead={markRead} 
                                onArchive={archiveNotification}
                                onDelete={removeNotification}
                            />
                            {index < filteredNotifications.length - 1 && (
                                <Divider sx={{ mx: 2.5 }} />
                            )}
                        </React.Fragment>
                    ))
                )}
            </Box>

            <Box sx={{ display: 'flex', justifyContent: 'space-between', px: 2 }}>
                <Button disabled={page <= 1 || inbox.isFetching} onClick={() => setPage((value) => value - 1)}>{t('common.previous', 'Previous')}</Button>
                <Button disabled={page >= (inbox.data?.totalPages ?? 1) || inbox.isFetching} onClick={() => setPage((value) => value + 1)}>{t('common.next', 'Next')}</Button>
            </Box>
            <Divider />
            <Box sx={{ py: 1.5, textAlign: 'center' }}>
                <Typography
                    component="button"
                    onClick={handleClose}
                    sx={{
                        fontSize: '0.875rem', fontWeight: 600, color: 'text.primary',
                        border: 'none', bgcolor: 'transparent', cursor: 'pointer', fontFamily: 'inherit',
                        '&:hover': { color: 'primary.main' },
                    }}
                >
                    {t('notifications.view_all', 'View All Notifications')}
                </Typography>
            </Box>
        </Drawer>
    );
};

