export interface BaseLocalEntity {
    id: string | number;
    dirty?: boolean;
}

export interface SaveBatchOptions<TLocal extends BaseLocalEntity, TDto, TServerResponse> {
    items: TLocal[];
    isCreate: (item: TLocal) => boolean;
    mapToDto: (item: TLocal) => TDto;
    createApi: (dtos: TDto[]) => Promise<TServerResponse[]>;
    updateApi: (dtos: TDto[]) => Promise<void>;
    updateLocalId: (item: TLocal, response: TServerResponse) => TLocal;
    markClean: (item: TLocal) => TLocal;
}

export async function processSaveBatch<TLocal extends BaseLocalEntity, TDto, TServerResponse>(
    options: SaveBatchOptions<TLocal, TDto, TServerResponse>
): Promise<TLocal[]> {
    const { items, isCreate, mapToDto, createApi, updateApi, updateLocalId, markClean } = options;
    
    const dirtyItems = items.filter((item) => item.dirty);
    if (dirtyItems.length === 0) return items;

    const creates = dirtyItems.filter(isCreate);
    const updates = dirtyItems.filter(i => !isCreate(i));

    let updatedItems = [...items];

    if (creates.length > 0) {
        const dtos = creates.map(mapToDto);
        const responses = await createApi(dtos);
        
        // Assuming responses come back in the same order
        updatedItems = updatedItems.map(item => {
            const createIdx = creates.findIndex((c) => c.id === item.id);
            if (createIdx >= 0 && responses[createIdx]) {
                return updateLocalId(item, responses[createIdx]);
            }
            return item;
        });
    }

    if (updates.length > 0) {
        const dtos = updates.map(mapToDto);
        await updateApi(dtos);
    }

    // Mark all processed items as clean
    const dirtyIds = new Set(dirtyItems.map((i) => i.id));
    return updatedItems.map(item => {
        if (dirtyIds.has(item.id)) {
            return markClean(item);
        }
        return item;
    });
}
