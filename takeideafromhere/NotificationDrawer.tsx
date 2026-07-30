import React, { useState, useMemo } from 'react';
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
    CircularProgress,
} from '@mui/material';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import SettingsIcon from '@mui/icons-material/Settings';
import CloseIcon from '@mui/icons-material/Close';
import InsertDriveFileIcon from '@mui/icons-material/InsertDriveFile';
import ArchiveIcon from '@mui/icons-material/ArchiveOutlined';
import DeleteIcon from '@mui/icons-material/DeleteOutline';
import { useUIStore } from '../../store/uiStore';
import { useNotificationStore } from '../../store/notificationStore';
import { useTranslation } from 'react-i18next';

import type { AppNotification } from '../../types';

const DRAWER_WIDTH = 380;

/**
 * Format relative time (e.g. "a few seconds", "1 day", "3 days")
 */
function timeAgo(dateStr: string): string {
    const now = Date.now();
    const diff = now - new Date(dateStr).getTime();
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (seconds < 60) return 'a few seconds';
    if (minutes < 60) return `${minutes} min`;
    if (hours < 24) return hours === 1 ? '1 hour' : `${hours} hours`;
    return days === 1 ? '1 day' : `${days} days`;
}

/**
 * Single notification item
 */
const NotificationItem: React.FC<{
    notification: AppNotification;
    onMarkRead: (id: string) => void;
    onArchive: (id: string) => void;
    onDelete: (id: string) => void;
}> = ({ notification, onMarkRead, onArchive, onDelete }) => {
    const n = notification;

    // Get priority border/badge color
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
                    bgcolor: '#f9fafb',
                    '& .hover-actions': { opacity: 1 }
                },
                position: 'relative',
            }}
        >
            {/* Priority Indicator Line */}
            <Box sx={{
                position: 'absolute',
                left: 0,
                top: 0,
                bottom: 0,
                width: 3,
                bgcolor: getPriorityColor(n.priority),
            }} />

            {/* Avatar */}
            <Avatar
                src={n.avatar}
                sx={{ width: 40, height: 40, flexShrink: 0, mt: 0.25 }}
            >
                {n.sender?.[0] || '?'}
            </Avatar>

            {/* Content */}
            <Box sx={{ flex: 1, minWidth: 0, pr: 4 }}>
                {/* Sender + message */}
                <Typography sx={{ fontSize: '0.8125rem', color: '#1e293b', lineHeight: 1.5 }}>
                    <strong>{n.sender}</strong> {n.message}
                </Typography>

                {/* Time + category */}
                <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', mt: 0.25 }}>
                    {timeAgo(n.createdAt)}
                    {n.category && ` \u00B7 ${n.category}`}
                    {n.priority && ` \u00B7 ${n.priority}`}
                </Typography>

                {/* Action buttons */}
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
                                    ...(action.variant === 'contained' && {
                                        bgcolor: '#1e293b',
                                        color: '#fff',
                                        '&:hover': { bgcolor: '#334155' },
                                    }),
                                    ...(action.variant === 'outlined' && {
                                        borderColor: '#e2e8f0',
                                        color: '#475569',
                                        '&:hover': { bgcolor: '#f8fafc', borderColor: '#cbd5e1' },
                                    }),
                                }}
                            >
                                {action.label}
                            </Button>
                        ))}
                    </Box>
                )}

                {/* Attachment */}
                {n.attachment && (
                    <Box sx={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1,
                        mt: 1,
                        p: 1,
                        border: '1px solid #e2e8f0',
                        borderRadius: '2px',
                        bgcolor: '#f8fafc',
                    }}>
                        <Box sx={{
                            width: 36,
                            height: 36,
                            borderRadius: '2px',
                            bgcolor: '#ef4444',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            flexShrink: 0,
                        }}>
                            <InsertDriveFileIcon sx={{ fontSize: 18, color: '#fff' }} />
                        </Box>
                        <Box sx={{ flex: 1, minWidth: 0 }}>
                            <Typography sx={{ fontSize: '0.75rem', color: '#1e293b', fontWeight: 500, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                {n.attachment.name}
                            </Typography>
                            <Typography sx={{ fontSize: '0.6875rem', color: '#94a3b8' }}>
                                {n.attachment.size}
                            </Typography>
                        </Box>
                        <Button
                            variant="outlined"
                            size="small"
                            sx={{
                                textTransform: 'none',
                                fontSize: '0.75rem',
                                fontWeight: 600,
                                borderRadius: '2px',
                                borderColor: '#e2e8f0',
                                color: '#475569',
                                minHeight: 28,
                                px: 1.5,
                                flexShrink: 0,
                                '&:hover': { bgcolor: '#f1f5f9', borderColor: '#cbd5e1' },
                            }}
                        >
                            Download
                        </Button>
                    </Box>
                )}
            </Box>

            {/* Unread dot */}
            {!n.read && (
                <Box sx={{
                    width: 8,
                    height: 8,
                    borderRadius: '50%',
                    bgcolor: '#3b82f6',
                    flexShrink: 0,
                    mt: 0.75,
                }} />
            )}

            {/* Hover Actions */}
            <Box
                className="hover-actions"
                onClick={(e) => e.stopPropagation()}
                sx={{
                    position: 'absolute',
                    top: 8,
                    right: 8,
                    display: 'flex',
                    gap: 0.5,
                    opacity: 0,
                    transition: 'opacity 0.15s',
                    bgcolor: 'rgba(255, 255, 255, 0.9)',
                    borderRadius: 1,
                    p: 0.25,
                    boxShadow: '0 2px 4px rgba(0,0,0,0.05)',
                }}
            >
                {!n.archived && (
                    <Tooltip title="Archive">
                        <IconButton size="small" onClick={() => onArchive(n.id)} sx={{ p: 0.5, color: '#64748b', '&:hover': { color: '#3b82f6' } }}>
                            <ArchiveIcon sx={{ fontSize: 16 }} />
                        </IconButton>
                    </Tooltip>
                )}
                <Tooltip title="Delete">
                    <IconButton size="small" onClick={() => onDelete(n.id)} sx={{ p: 0.5, color: '#64748b', '&:hover': { color: '#ef4444' } }}>
                        <DeleteIcon sx={{ fontSize: 16 }} />
                    </IconButton>
                </Tooltip>
            </Box>
        </Box>
    );
};

/**
 * Notification drawer - slides from right
 */
const NotificationDrawer: React.FC = () => {
    const { t } = useTranslation();
    const notificationDrawerOpen = useUIStore((s) => s.notificationDrawerOpen);
    const setNotificationDrawerOpen = useUIStore((s) => s.setNotificationDrawerOpen);
    const notifications = useNotificationStore((s) => s.notifications);
    const unreadCount = useNotificationStore((s) => s.unreadCount);
    const loading = useNotificationStore((s) => s.loading);
    const markAllRead = useNotificationStore((s) => s.markAllRead);
    const markRead = useNotificationStore((s) => s.markRead);
    const archiveNotification = useNotificationStore((s) => s.archiveNotification);
    const removeNotification = useNotificationStore((s) => s.removeNotification);
    const [tab, setTab] = useState(0);

    const allNotifications = useMemo(() => notifications.filter((n) => !n.archived), [notifications]);
    const unreadNotifications = useMemo(() => notifications.filter((n) => !n.read && !n.archived), [notifications]);
    const archivedNotifications = useMemo(() => notifications.filter((n) => n.archived), [notifications]);

    const filteredNotifications = tab === 0 ? allNotifications : tab === 1 ? unreadNotifications : archivedNotifications;

    const handleClose = () => setNotificationDrawerOpen(false);

    return (
        <Drawer
            anchor="right"
            open={notificationDrawerOpen}
            onClose={handleClose}
            PaperProps={{
                sx: {
                    width: { xs: '100vw', sm: DRAWER_WIDTH },
                    maxWidth: '100vw',
                    borderRadius: 0,
                    boxShadow: '-4px 0 24px rgba(0,0,0,0.08)',
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
                pt: 2,
                pb: 1,
            }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography sx={{ fontSize: '1.125rem', fontWeight: 700, color: '#1e293b' }}>
                        {t('common.notifications')}
                    </Typography>
                    {loading && <CircularProgress size={16} sx={{ color: '#64748b' }} />}
                </Box>
                <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Tooltip title={t('notifications.mark_all_read')}>
                        <IconButton
                            size="small"
                            onClick={markAllRead}
                            sx={{ color: '#64748b', '&:hover': { color: '#10b981' } }}
                        >
                            <DoneAllIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.settings')}>
                        <IconButton size="small" sx={{ color: '#64748b' }}>
                            <SettingsIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>
                    <Tooltip title={t('common.close')}>
                        <IconButton size="small" onClick={handleClose} sx={{ color: '#64748b' }}>
                            <CloseIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>
                </Box>
            </Box>

            {/* Tabs */}
            <Box sx={{ px: 2.5 }}>
                <Tabs
                    value={tab}
                    onChange={(_, v) => setTab(v)}
                    sx={{
                        minHeight: 36,
                        '& .MuiTabs-indicator': { display: 'none' },
                        '& .MuiTab-root': {
                            textTransform: 'none',
                            minHeight: 32,
                            fontSize: '0.8125rem',
                            fontWeight: 600,
                            color: '#64748b',
                            px: 1.5,
                            py: 0.5,
                            mr: 0.5,
                            minWidth: 'auto',
                            '&.Mui-selected': {
                                color: '#1e293b',
                            },
                        },
                    }}
                >
                    <Tab label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                            {t('notifications.all')}
                            <Box sx={{
                                bgcolor: tab === 0 ? '#1e293b' : '#e2e8f0',
                                color: tab === 0 ? '#fff' : '#64748b',
                                fontSize: '0.6875rem',
                                fontWeight: 700,
                                borderRadius: '2px',
                                minWidth: 20,
                                height: 20,
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                px: 0.5,
                             }}>
                                {allNotifications.length}
                            </Box>
                        </Box>
                    } />
                    <Tab label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                            {t('notifications.unread')}
                            <Box sx={{
                                bgcolor: tab === 1 ? '#1e293b' : '#e2e8f0',
                                color: tab === 1 ? '#fff' : '#64748b',
                                fontSize: '0.6875rem',
                                fontWeight: 700,
                                borderRadius: '2px',
                                minWidth: 20,
                                height: 20,
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                px: 0.5,
                            }}>
                                {unreadCount}
                            </Box>
                        </Box>
                    } />
                    <Tab label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                            {t('notifications.archived')}
                            <Box sx={{
                                bgcolor: tab === 2 ? '#1e293b' : '#e2e8f0',
                                color: tab === 2 ? '#fff' : '#64748b',
                                fontSize: '0.6875rem',
                                fontWeight: 700,
                                borderRadius: '2px',
                                minWidth: 20,
                                height: 20,
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                px: 0.5,
                            }}>
                                {archivedNotifications.length}
                            </Box>
                        </Box>
                    } />
                </Tabs>
            </Box>

            <Divider sx={{ mt: 1 }} />

            {/* Notification list */}
            <Box sx={{
                flex: 1,
                overflowY: 'auto',
                '&::-webkit-scrollbar': { width: '4px' },
                '&::-webkit-scrollbar-thumb': { bgcolor: '#cbd5e1', borderRadius: '2px' },
            }}>
                {filteredNotifications.length === 0 ? (
                    <Box sx={{ py: 6, textAlign: 'center' }}>
                        <Typography sx={{ fontSize: '0.875rem', color: '#94a3b8' }}>
                            {t('notifications.no_notifications')}
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

            {/* Footer */}
            <Divider />
            <Box sx={{
                py: 1.5,
                textAlign: 'center',
            }}>
                <Typography
                    component="button"
                    onClick={handleClose}
                    sx={{
                        fontSize: '0.875rem',
                        fontWeight: 600,
                        color: '#1e293b',
                        border: 'none',
                        bgcolor: 'transparent',
                        cursor: 'pointer',
                        fontFamily: 'inherit',
                        '&:hover': { color: '#6366f1' },
                    }}
                >
                    {t('notifications.view_all')}
                </Typography>
            </Box>
        </Drawer>
    );
};

export default NotificationDrawer;
