import React, { useMemo, useEffect, useRef } from 'react';
import { Box, Typography, Paper, Chip, Stack } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { useProcessBuilderData } from '../../hooks/useProcessBuilderData';
import {
    ReactFlow,
    MiniMap,
    Controls,
    Background,
    MarkerType,
    Handle,
    Position,
    useNodesState,
    useEdgesState,
} from '@xyflow/react';
import type { NodeProps, Node, Edge } from '@xyflow/react';
import '@xyflow/react/dist/style.css';

// ─────────────────────────────────────────────────────────────────────────────
// START node
// ─────────────────────────────────────────────────────────────────────────────
const StartNode = () => (
    <Box sx={{ position: 'relative', display: 'inline-flex', alignItems: 'center', justifyContent: 'center' }}>
        <Handle type="source" position={Position.Bottom} style={{ background: '#16a34a' }} />
        <Box sx={{ px: 4, py: 0.75, bgcolor: '#dcfce7', border: '2px solid #16a34a', borderRadius: '999px' }}>
            <Typography sx={{ fontWeight: 700, fontSize: '0.8rem', color: '#14532d', letterSpacing: 1 }}>
                START
            </Typography>
        </Box>
    </Box>
);

// ─────────────────────────────────────────────────────────────────────────────
// REQUEST FORM node — cyan parallelogram
// Bullet-dots are purple when the control has a transition defined.
// ─────────────────────────────────────────────────────────────────────────────
type RequestFormControlItem = { id: string; label: string; type: string; hasTransition: boolean };
type RequestFormNodeData   = { processName: string; controls: RequestFormControlItem[] };

const RequestFormNode = ({ data }: NodeProps<Node<RequestFormNodeData>>) => (
    <Paper variant="outlined" sx={{
        minWidth: 240, maxWidth: 300,
        borderColor: '#0ea5e9', borderWidth: 2,
        bgcolor: 'background.paper', overflow: 'hidden', borderRadius: 1,
    }}>
        <Handle type="target" position={Position.Top}    style={{ background: '#0ea5e9' }} />
        <Handle type="source" position={Position.Bottom} style={{ background: '#0ea5e9' }} />

        <Box sx={{
            px: 2, py: 1, bgcolor: '#0ea5e9', textAlign: 'center',
            clipPath: 'polygon(0 0, 100% 0, 93% 100%, 7% 100%)',
        }}>
            <Typography variant="caption" sx={{ fontWeight: 700, color: '#fff', letterSpacing: 0.5 }}>
                {data.processName || 'Request Form'}
            </Typography>
        </Box>

        <Stack spacing={0.5} sx={{ p: 1.5 }}>
            {data.controls.length === 0 && (
                <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.7rem', textAlign: 'center' }}>
                    No controls defined
                </Typography>
            )}
            {data.controls.map((ctrl) => (
                <Stack key={ctrl.id} direction="row" spacing={0.75} alignItems="center">
                    <Box sx={{
                        width: 6, height: 6, borderRadius: '50%', flexShrink: 0,
                        bgcolor: ctrl.hasTransition ? '#7c3aed' : '#0ea5e9',
                    }} />
                    <Typography variant="caption" noWrap sx={{ fontSize: '0.7rem', flex: 1 }}>
                        {ctrl.label}
                    </Typography>
                    {ctrl.hasTransition && (
                        <Typography variant="caption" sx={{ fontSize: '0.6rem', color: '#7c3aed', fontWeight: 700, flexShrink: 0 }}>
                            →
                        </Typography>
                    )}
                    <Typography variant="caption" sx={{ fontSize: '0.65rem', color: 'text.secondary', flexShrink: 0 }}>
                        {ctrl.type}
                    </Typography>
                </Stack>
            ))}
        </Stack>
    </Paper>
);

// ─────────────────────────────────────────────────────────────────────────────
// DIAMOND node — shared shape, colour passed via data
// ─────────────────────────────────────────────────────────────────────────────
type DiamondNodeData = { label: string; color: string; bgColor: string; textColor: string };

const DiamondNode = ({ data }: NodeProps<Node<DiamondNodeData>>) => (
    <Box sx={{ position: 'relative', width: 140, height: 140, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Handle type="target" position={Position.Top}    style={{ top: 0,    background: data.color }} />
        <Handle type="source" position={Position.Bottom} style={{ bottom: 0, background: data.color }} />
        <Handle type="source" position={Position.Left}   style={{ left: 0,   background: data.color }} id="left" />
        <Handle type="source" position={Position.Right}  style={{ right: 0,  background: data.color }} id="right" />

        <Box sx={{
            width: 98, height: 98,
            bgcolor: data.bgColor,
            border: `2px solid ${data.color}`,
            transform: 'rotate(45deg)',
            position: 'absolute',
            borderRadius: 1,
        }} />
        <Typography sx={{
            position: 'relative', zIndex: 1,
            fontSize: '0.68rem', fontWeight: 700, color: data.textColor,
            textAlign: 'center', px: 2, lineHeight: 1.35,
            whiteSpace: 'pre-line',
        }}>
            {data.label}
        </Typography>
    </Box>
);

// ─────────────────────────────────────────────────────────────────────────────
// STEP node
// ─────────────────────────────────────────────────────────────────────────────
type StepNodeData = {
    name: string;
    order: number;
    status: string;
    isActive: boolean;
    activitiesCount: number;
    activityNames: string[];
    hasActivityTransitions: boolean;
};

const StepNode = ({ data }: NodeProps<Node<StepNodeData>>) => (
    <Paper variant="outlined" sx={{
        p: 1.5, minWidth: 200,
        borderColor: data.isActive ? 'primary.main' : 'divider',
        borderWidth: 2,
        boxShadow: '0 4px 12px rgba(0,0,0,0.05)',
        bgcolor: 'background.paper', borderRadius: 1,
    }}>
        <Handle type="target" position={Position.Top}    style={{ background: '#6366f1' }} />
        <Handle type="source" position={Position.Bottom} style={{ background: '#6366f1' }} />

        <Stack spacing={1}>
            <Stack direction="row" alignItems="center" justifyContent="space-between">
                <Chip label={`#${data.order}`} size="small" color="primary" />
                <Stack direction="row" spacing={0.5}>
                    {data.hasActivityTransitions && (
                        <Chip label="⤳" size="small" sx={{ bgcolor: '#fff7ed', color: '#f97316', fontWeight: 700, border: '1px solid #f97316', minWidth: 24, px: 0.5 }} />
                    )}
                    <Chip
                        label={data.status}
                        size="small"
                        color={
                            data.status === 'completed' ? 'success'
                            : data.status === 'active'  ? 'info'
                            : data.status === 'skipped' ? 'default' : 'warning'
                        }
                    />
                </Stack>
            </Stack>
            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{data.name}</Typography>
            {data.activitiesCount > 0 && (
                <Box sx={{ p: 1, bgcolor: 'grey.50', borderRadius: 1, border: '1px dashed', borderColor: 'divider' }}>
                    <Typography variant="caption" color="text.secondary" display="block">
                        {data.activitiesCount} {data.activitiesCount === 1 ? 'Activity' : 'Activities'}
                    </Typography>
                    {data.activityNames.map((name, i) => (
                        <Typography key={i} variant="caption" display="block" noWrap>• {name}</Typography>
                    ))}
                </Box>
            )}
        </Stack>
    </Paper>
);

const nodeTypes = {
    startNode:       StartNode,
    requestFormNode: RequestFormNode,
    decisionNode:    DiamondNode,
    stepNode:        StepNode,
};

// ─────────────────────────────────────────────────────────────────────────────
// Edge helper
// ─────────────────────────────────────────────────────────────────────────────
const mkEdge = (
    id: string,
    source: string,
    target: string,
    opts: { label?: string; color: string; dashed?: boolean; animated?: boolean; sourceHandle?: string }
): Edge => ({
    id,
    source,
    target,
    sourceHandle: opts.sourceHandle,
    label: opts.label,
    labelBgPadding:      [5, 3] as [number, number],
    labelBgBorderRadius: 4,
    labelBgStyle:        { fillOpacity: 0.92 },
    labelStyle:          { fontSize: 10, fill: opts.color, fontWeight: 600 },
    markerEnd:           { type: MarkerType.ArrowClosed, color: opts.color },
    style: {
        stroke: opts.color,
        strokeWidth: 2,
        strokeDasharray: opts.dashed ? '5 3' : undefined,
    },
    animated: opts.animated ?? false,
});

// ─────────────────────────────────────────────────────────────────────────────
// WorkflowDiagramTab
// ─────────────────────────────────────────────────────────────────────────────
export const WorkflowDiagramTab: React.FC = () => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    const { id: routeId } = useParams<{ id: string }>();
    const isEditMode = !!routeId && routeId !== 'new';

    const { steps, transitions, requestControls, variables, processInfo } = useProcessBuilderContext();
    const { operators } = useProcessBuilderData(processInfo.id, isEditMode, true);

    // ── Categorise transitions ────────────────────────────────────────────────
    const activeTransitions = useMemo(
        () => transitions.filter((tr) => tr.isActive && tr.stepId),
        [transitions]
    );

    // Request-control & process-level transitions (no activityControlId)
    const reqTransitions = useMemo(
        () => activeTransitions.filter((tr) => !tr.activityControlId),
        [activeTransitions]
    );

    // Activity-control transitions (have activityControlId)
    const actTransitions = useMemo(
        () => activeTransitions.filter((tr) => !!tr.activityControlId),
        [activeTransitions]
    );

    // Map: local step.id → activity-control transitions whose source control lives in that step
    const actTransitionsByStepId = useMemo(() => {
        const map = new Map<string, typeof actTransitions>();
        for (const tr of actTransitions) {
            let sourceStepId: string | undefined;
            if (tr.activityId) {
                for (const s of steps) {
                    if (s.activities.some((a) => a.id === tr.activityId)) { sourceStepId = s.id; break; }
                }
            }
            if (!sourceStepId && tr.activityControlId) {
                outer: for (const s of steps) {
                    for (const a of s.activities) {
                        if (a.controls.some((c) => c.id === tr.activityControlId)) { sourceStepId = s.id; break outer; }
                    }
                }
            }
            if (sourceStepId) {
                if (!map.has(sourceStepId)) map.set(sourceStepId, []);
                map.get(sourceStepId)!.push(tr);
            }
        }
        return map;
    }, [actTransitions, steps]);

    // Set of request-control ids that have at least one transition
    const reqCtrlIdsWithTransitions = useMemo(
        () => new Set(reqTransitions.map((tr) => tr.requestControlId).filter(Boolean)),
        [reqTransitions]
    );

    // ── Build a transition edge label ─────────────────────────────────────────
    const buildLabel = (tr: (typeof activeTransitions)[number], idx: number) => {
        let subject = '';
        if (tr.requestControlId) {
            const ctrl = requestControls.find((c) => c.id === tr.requestControlId);
            if (ctrl) subject = isRtl ? (ctrl.labelAR || ctrl.label) : ctrl.label;
        } else if (tr.activityControlId) {
            outer: for (const s of steps) {
                for (const a of s.activities) {
                    const ctrl = a.controls.find((c) => c.id === tr.activityControlId);
                    if (ctrl) { subject = isRtl ? (ctrl.labelAR || ctrl.label) : ctrl.label; break outer; }
                }
            }
        }
        if (!subject && tr.variableId) {
            const v = variables.find((x) => x.id === tr.variableId);
            if (v) subject = isRtl ? (v.nameAR || v.name) : v.name;
        }
        const op = operators.find((o) => o.id === tr.operatorId);
        const opLabel = op ? (isRtl ? op.nameAR || op.name : op.name) : '=';
        return subject
            ? `${subject} ${opLabel} ${tr.value || '?'}`
            : `${t('workflow.condition', 'Condition')} ${idx + 1}`;
    };

    // ── NODES ────────────────────────────────────────────────────────────────
    const computedNodes = useMemo<Node[]>(() => {
        const sortedSteps = [...steps].sort((a, b) => a.order - b.order);
        const hasReqTr = reqTransitions.length > 0;

        let y = 20;
        const nodes: Node[] = [];

        // Start
        nodes.push({ id: '__start__', type: 'startNode', position: { x: 315, y }, data: {} });
        y += 80;

        // Request Form
        nodes.push({
            id: '__form__',
            type: 'requestFormNode',
            position: { x: 250, y },
            data: {
                processName: isRtl ? (processInfo.nameAR || processInfo.name) : processInfo.name,
                controls: requestControls.map((c) => ({
                    id: c.id,
                    label: isRtl ? (c.labelAR || c.label) : c.label,
                    type: c.type,
                    hasTransition: reqCtrlIdsWithTransitions.has(c.id),
                })),
            } satisfies RequestFormNodeData,
        });
        y += 200;

        // Request-level decision diamond (yellow)
        if (hasReqTr) {
            nodes.push({
                id: '__req_dec__',
                type: 'decisionNode',
                position: { x: 285, y },
                data: {
                    label: t('workflow.evaluate_conditions', 'Evaluate\nConditions'),
                    color: '#eab308', bgColor: '#fef9c3', textColor: '#713f12',
                } satisfies DiamondNodeData,
            });
            y += 180;
        }

        // Steps + optional per-step activity-decision diamonds
        for (const step of sortedSteps) {
            const stepActTrs = actTransitionsByStepId.get(step.id);
            nodes.push({
                id: step.id,
                type: 'stepNode',
                position: { x: 250, y },
                data: {
                    name: step.name,
                    order: step.order,
                    status: step.status,
                    isActive: step.isActive ?? true,
                    activitiesCount: step.activities.length,
                    activityNames: step.activities.map((a) => a.name),
                    hasActivityTransitions: !!stepActTrs?.length,
                } satisfies StepNodeData,
            });
            y += 220;

            if (stepActTrs?.length) {
                nodes.push({
                    id: `__act_dec_${step.id}__`,
                    type: 'decisionNode',
                    position: { x: 285, y },
                    data: {
                        label: t('workflow.activity_conditions', 'Activity\nConditions'),
                        color: '#f97316', bgColor: '#fff7ed', textColor: '#7c2d12',
                    } satisfies DiamondNodeData,
                });
                y += 180;
            }
        }

        return nodes;
    }, [steps, requestControls, reqTransitions, reqCtrlIdsWithTransitions, actTransitionsByStepId, processInfo, isRtl, t]);

    // ── EDGES ────────────────────────────────────────────────────────────────
    const computedEdges = useMemo<Edge[]>(() => {
        const sortedSteps = [...steps].sort((a, b) => a.order - b.order);
        const result: Edge[] = [];
        const hasReqTr = reqTransitions.length > 0;

        // Start → Form
        result.push(mkEdge('start-form', '__start__', '__form__', { color: '#16a34a' }));
        if (sortedSteps.length === 0) return result;

        // ── Request-level transitions ─────────────────────────────────────────
        if (hasReqTr) {
            result.push(mkEdge('form-req-dec', '__form__', '__req_dec__', { color: '#0ea5e9' }));

            reqTransitions.forEach((tr, idx) => {
                result.push(mkEdge(`req-tr-${tr.id}`, '__req_dec__', tr.stepId, {
                    label: buildLabel(tr, idx),
                    color: '#7c3aed', animated: true,
                    sourceHandle: idx % 2 === 0 ? 'right' : 'left',
                }));
            });

            // Default edge always shown — represents the fallthrough path when no condition matches
            result.push(mkEdge('req-default', '__req_dec__', sortedSteps[0].id, {
                label: t('workflow.default', 'Default'),
                color: '#64748b', dashed: true,
            }));
        } else {
            // No transitions → Form → Step 1 default
            result.push(mkEdge('form-step1', '__form__', sortedSteps[0].id, {
                label: t('workflow.default', 'Default'),
                color: '#64748b', dashed: true,
            }));
        }

        // ── Per-step activity-control transitions ─────────────────────────────
        for (let i = 0; i < sortedSteps.length; i++) {
            const step     = sortedSteps[i];
            const nextStep = sortedSteps[i + 1];
            const stepActTrs = actTransitionsByStepId.get(step.id);
            const actDecId = `__act_dec_${step.id}__`;

            if (stepActTrs?.length) {
                // Step → Activity-decision
                result.push(mkEdge(`step-act-dec-${step.id}`, step.id, actDecId, { color: '#f97316' }));

                // Activity-decision → conditional target steps
                stepActTrs.forEach((tr, idx) => {
                    result.push(mkEdge(`act-tr-${tr.id}`, actDecId, tr.stepId, {
                        label: buildLabel(tr, idx),
                        color: '#f97316', animated: true,
                        sourceHandle: idx % 2 === 0 ? 'right' : 'left',
                    }));
                });

                // Default: Activity-decision → next step
                if (nextStep) {
                    result.push(mkEdge(`act-default-${step.id}`, actDecId, nextStep.id, {
                        label: t('workflow.default', 'Default'),
                        color: '#64748b', dashed: true,
                    }));
                }
            } else if (nextStep) {
                // Sequential: step → next step
                result.push(mkEdge(`seq-${step.id}-${nextStep.id}`, step.id, nextStep.id, {
                    label: t('workflow.next', 'Next'),
                    color: '#94a3b8', dashed: true,
                }));
            }
        }

        return result;
    }, [steps, reqTransitions, actTransitionsByStepId, requestControls, variables, operators, isRtl, t, buildLabel]);

    // ── SYNC into ReactFlow state ─────────────────────────────────────────────
    const [rfNodes, setRfNodes, onNodesChange] = useNodesState<Node>(computedNodes);
    const [rfEdges, setRfEdges, onEdgesChange] = useEdgesState<Edge>(computedEdges);

    const nodesKeyRef = useRef('');
    const edgesKeyRef = useRef('');

    useEffect(() => {
        const key = computedNodes.map((n) => `${n.id}:${JSON.stringify(n.data)}`).join('|');
        if (key !== nodesKeyRef.current) { nodesKeyRef.current = key; setRfNodes(computedNodes); }
    }, [computedNodes, setRfNodes]);

    useEffect(() => {
        const key = computedEdges.map((e) => `${e.id}:${String(e.label)}`).join('|');
        if (key !== edgesKeyRef.current) { edgesKeyRef.current = key; setRfEdges(computedEdges); }
    }, [computedEdges, setRfEdges]);

    // ── RENDER ───────────────────────────────────────────────────────────────
    return (
        <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
            {/* Legend */}
            <Box sx={{ p: 2, borderBottom: '1px solid', borderColor: 'divider', display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                    {t('workflow.diagram', 'Workflow Diagram')}
                </Typography>
                <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <Box sx={{ width: 28, height: 14, bgcolor: '#dcfce7', border: '2px solid #16a34a', borderRadius: '999px' }} />
                        <Typography variant="caption" color="text.secondary">Start</Typography>
                    </Stack>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <Box sx={{ width: 28, height: 16, bgcolor: '#e0f2fe', border: '2px solid #0ea5e9', clipPath: 'polygon(7% 0%,100% 0%,93% 100%,0% 100%)' }} />
                        <Typography variant="caption" color="text.secondary">Request Form</Typography>
                    </Stack>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <Box sx={{ width: 16, height: 16, bgcolor: '#fef9c3', border: '2px solid #eab308', transform: 'rotate(45deg)' }} />
                        <Typography variant="caption" color="text.secondary">Request Condition</Typography>
                    </Stack>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <Box sx={{ width: 16, height: 16, bgcolor: '#fff7ed', border: '2px solid #f97316', transform: 'rotate(45deg)' }} />
                        <Typography variant="caption" color="text.secondary">Activity Condition</Typography>
                    </Stack>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <Box sx={{ width: 28, height: 16, bgcolor: 'background.paper', border: '2px solid #6366f1', borderRadius: 0.5 }} />
                        <Typography variant="caption" color="text.secondary">Process Step</Typography>
                    </Stack>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <Box sx={{ width: 20, height: 2, bgcolor: '#7c3aed', borderRadius: 1 }} />
                        <Typography variant="caption" color="text.secondary">Request Transition</Typography>
                    </Stack>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <Box sx={{ width: 20, height: 2, bgcolor: '#f97316', borderRadius: 1 }} />
                        <Typography variant="caption" color="text.secondary">Activity Transition</Typography>
                    </Stack>
                </Stack>
            </Box>

            <Box sx={{ flexGrow: 1, bgcolor: '#f8fafc' }}>
                <ReactFlow
                    nodes={rfNodes}
                    edges={rfEdges}
                    onNodesChange={onNodesChange}
                    onEdgesChange={onEdgesChange}
                    nodeTypes={nodeTypes}
                    nodesConnectable={false}
                    fitView
                    attributionPosition="bottom-right"
                >
                    <MiniMap
                        nodeStrokeColor={(n) =>
                            n.type === 'startNode'       ? '#16a34a' :
                            n.type === 'requestFormNode' ? '#0ea5e9' :
                            n.type === 'decisionNode'    ? ((n.data as DiamondNodeData).color ?? '#eab308') : '#6366f1'
                        }
                        nodeColor={() => '#fff'}
                        nodeBorderRadius={4}
                    />
                    <Controls />
                    <Background color="#d1d5db" gap={16} />
                </ReactFlow>
            </Box>
        </Box>
    );
};

export default WorkflowDiagramTab;
