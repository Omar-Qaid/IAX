import React from 'react';
import {
    TextFields, ArrowDropDownCircle, CheckBox as CheckBoxIcon,
    CalendarMonth, CloudUpload, Draw, Search as SearchIcon, GridOn,
    HowToReg, RateReview, Edit as EditIcon, Api, Notifications,
} from '@mui/icons-material';
import type { ControlType, ActivityType, ActionType } from '../types';

export interface ControlPaletteEntry { type: ControlType; label: string; icon: React.ReactNode }
export interface ActivityTypeEntry { type: ActivityType; label: string; icon: React.ReactNode }

export const CONTROL_PALETTE: ControlPaletteEntry[] = [
    { type: 'digits', label: 'Digits', icon: <TextFields fontSize="small" /> },
    { type: 'text', label: 'Text', icon: <TextFields fontSize="small" /> },
    { type: 'longtext', label: 'Long text', icon: <TextFields fontSize="small" /> },
    { type: 'date', label: 'Date', icon: <CalendarMonth fontSize="small" /> },
    { type: 'time', label: 'Time', icon: <CalendarMonth fontSize="small" /> },
    { type: 'url', label: 'Url', icon: <SearchIcon fontSize="small" /> },
    { type: 'dropdown-db', label: 'Drop Down List (Fill From DataBase)', icon: <ArrowDropDownCircle fontSize="small" /> },
    { type: 'dropdown-manual', label: 'Drop Down List (Fill Manually)', icon: <ArrowDropDownCircle fontSize="small" /> },
    { type: 'checkbox', label: 'Check box', icon: <CheckBoxIcon fontSize="small" /> },
    { type: 'checkboxlist', label: 'CheckBoxList', icon: <CheckBoxIcon fontSize="small" /> },
    { type: 'radiobuttonlist', label: 'RadioButtonList', icon: <CheckBoxIcon fontSize="small" /> },
    { type: 'table', label: 'Table', icon: <GridOn fontSize="small" /> },
    { type: 'label', label: 'Label', icon: <TextFields fontSize="small" /> },
    { type: 'employeesearch', label: 'EmployeeSearch', icon: <HowToReg fontSize="small" /> },
    { type: 'employeeid', label: 'EmployeeID', icon: <HowToReg fontSize="small" /> },
    { type: 'file', label: 'File', icon: <CloudUpload fontSize="small" /> },
    { type: 'showroom', label: 'Showroom', icon: <SearchIcon fontSize="small" /> },
    { type: 'signature', label: 'Signature', icon: <Draw fontSize="small" /> },
    { type: 'location', label: 'Location', icon: <SearchIcon fontSize="small" /> },
    { type: 'advertiser', label: 'Advertiser', icon: <SearchIcon fontSize="small" /> },
];

export const ACTIVITY_TYPES: ActivityTypeEntry[] = [
    { type: 'approval', label: 'Approval', icon: <HowToReg fontSize="small" /> },
    { type: 'review', label: 'Review', icon: <RateReview fontSize="small" /> },
    { type: 'data-entry', label: 'Data Entry', icon: <EditIcon fontSize="small" /> },
    { type: 'api', label: 'API Action', icon: <Api fontSize="small" /> },
    { type: 'notification', label: 'Notification', icon: <Notifications fontSize="small" /> },
];

export const getControlIcon = (type: ControlType): React.ReactNode => {
    const entry = CONTROL_PALETTE.find(c => c.type === type);
    return entry?.icon ?? <TextFields fontSize="small" />;
};

export const getActivityIcon = (type: ActivityType): React.ReactNode => {
    const entry = ACTIVITY_TYPES.find(a => a.type === type);
    return entry?.icon ?? <HowToReg fontSize="small" />;
};

export const ACTION_TYPES: ActionType[] = ['approve', 'reject', 'return', 'escalate'];
