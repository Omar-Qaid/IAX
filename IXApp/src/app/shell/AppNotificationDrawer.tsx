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

// Mock data to demonstrate the UI structure without a dedicated store
const MOCK_NOTIFICATIONS: AppDrawerNotification[] = [
    {
        id: '1',
        sender: 'System Admin',
        message: 'Your data export has completed successfully.',
        createdAt: new Date(Date.now() - 1000 * 60 * 5).toISOString(),
        read: false,
        archived: false,
        priority: 'Medium',
        category: 'System',
        actions: [{ label: 'View File', variant: 'contained' }],
        attachment: { name: 'export_data.csv', size: '2.4 MB' }
    },
    {
        id: '2',
        sender: 'Security',
        message: 'Multiple failed login attempts detected.',
        createdAt: new Date(Date.now() - 1000 * 60 * 60 * 2).toISOString(),
        read: false,
        archived: false,
        priority: 'Critical',
        category: 'Alert'
    }
];

// ─── Utilities ───────────────────────────────────────────────────────────────

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

// ─── Notification Item ───────────────────────────────────────────────────────

const NotificationItem: React.FC<{
    notification: AppDrawerNotification;
    onMarkRead: (id: string) => void;
    onArchive: (id: string) => void;
    onDelete: (id: string) => void;
}> = ({ notification, onMarkRead, onArchive, onDelete }) => {
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
                position: 'absolute', left: 0, top: 0, bottom: 0, width: 3,
                bgcolor: getPriorityColor(n.priority),
            }} />

            <Avatar src={n.avatar} sx={{ width: 40, height: 40, flexShrink: 0, mt: 0.25 }}>
                {n.sender?.[0] || '?'}
            </Avatar>

            <Box sx={{ flex: 1, minWidth: 0, pr: 4 }}>
                <Typography sx={{ fontSize: '0.8125rem', color: 'text.primary', lineHeight: 1.5 }}>
                    <strong>{n.sender}</strong> {n.message}
                </Typography>
                <Typography sx={{ fontSize: '0.75rem', color: 'text.secondary', mt: 0.25 }}>
                    {timeAgo(n.createdAt)}
                    {n.category && ` \u00B7 ${n.category}`}
                    {n.priority && ` \u00B7 ${n.priority}`}
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
                                {action.label}
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
                            Download
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
                    position: 'absolute', top: 8, right: 8, display: 'flex', gap: 0.5,
                    opacity: 0, transition: 'opacity 0.15s', bgcolor: 'background.paper',
                    borderRadius: 1, p: 0.25, boxShadow: '0 2px 4px rgba(0,0,0,0.05)',
                }}
            >
                {!n.archived && (
                    <Tooltip title="Archive">
                        <IconButton size="small" onClick={() => onArchive(n.id)} sx={{ p: 0.5 }}>
                            <ArchiveIcon sx={{ fontSize: 16 }} />
                        </IconButton>
                    </Tooltip>
                )}
                <Tooltip title="Delete">
                    <IconButton size="small" onClick={() => onDelete(n.id)} sx={{ p: 0.5 }}>
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
    
    // For demonstration, use local state rather than breaking existing toast notifications
    const [notifications, setNotifications] = useState<AppDrawerNotification[]>(MOCK_NOTIFICATIONS);
    const [tab, setTab] = useState(0);

    const allNotifications = useMemo(() => notifications.filter((n) => !n.archived), [notifications]);
    const unreadNotifications = useMemo(() => notifications.filter((n) => !n.read && !n.archived), [notifications]);
    const archivedNotifications = useMemo(() => notifications.filter((n) => n.archived), [notifications]);

    const filteredNotifications = tab === 0 ? allNotifications : tab === 1 ? unreadNotifications : archivedNotifications;
    const unreadCount = unreadNotifications.length;

    const handleClose = () => setNotificationDrawerOpen(false);

    const markRead = (id: string) => setNotifications(prev => prev.map(n => n.id === id ? { ...n, read: true } : n));
    const markAllRead = () => setNotifications(prev => prev.map(n => ({ ...n, read: true })));
    const archiveNotification = (id: string) => setNotifications(prev => prev.map(n => n.id === id ? { ...n, archived: true } : n));
    const removeNotification = (id: string) => setNotifications(prev => prev.filter(n => n.id !== id));

    return (
        <Drawer
            anchor="right"
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
                    onChange={(_, v) => setTab(v)}
                    sx={{
                        minHeight: 36,
                        '& .MuiTabs-indicator': { display: 'none' },
                        '& .MuiTab-root': {
                            textTransform: 'none', minHeight: 32, fontSize: '0.8125rem', fontWeight: 600,
                            color: 'text.secondary', px: 1.5, py: 0.5, mr: 0.5, minWidth: 'auto',
                            '&.Mui-selected': { color: 'text.primary' },
                        },
                    }}
                >
                    <Tab label={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                            {t('notifications.all', 'All')}
                            <Box sx={{
                                bgcolor: tab === 0 ? 'text.primary' : 'action.selected',
                                color: tab === 0 ? 'background.paper' : 'text.secondary',
                                fontSize: '0.6875rem', fontWeight: 700, borderRadius: '2px',
                                minWidth: 20, height: 20, display: 'flex', alignItems: 'center', justifyContent: 'center', px: 0.5,
                             }}>
                                {allNotifications.length}
                            </Box>
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
                            <Box sx={{
                                bgcolor: tab === 2 ? 'text.primary' : 'action.selected',
                                color: tab === 2 ? 'background.paper' : 'text.secondary',
                                fontSize: '0.6875rem', fontWeight: 700, borderRadius: '2px',
                                minWidth: 20, height: 20, display: 'flex', alignItems: 'center', justifyContent: 'center', px: 0.5,
                            }}>
                                {archivedNotifications.length}
                            </Box>
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

