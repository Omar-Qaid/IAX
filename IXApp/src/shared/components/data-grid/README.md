# AdvancedDataGrid Features & Specifications

This document outlines the core features, keyboard shortcuts, and specific behaviors implemented in the `AdvancedDataGrid` component. **Do not remove or alter these features without explicit approval**, as they form the core user experience for fast, Excel-like data entry.

## 1. Excel-Like Keyboard Navigation
The grid provides a robust, keyboard-first navigation experience similar to Excel:
- **Arrow Keys (`Up`, `Down`, `Left`, `Right`)**: Navigate focus seamlessly between individual cells.
- **`Tab` / `Shift+Tab`**: Moves focus to the next or previous cell. Automatically wraps to the next/previous row when reaching the edge.
- **`Home` / `End`**: Instantly jumps to the first or last column in the current row.
- **`PageUp` / `PageDown`**: Jumps up or down by 10 rows for fast scrolling.

## 2. Inline Row Editing Workflow
The inline editing system is optimized to never require a mouse click:
- **Start Editing (`Enter` or `F2`)**: Pressing `Enter` or `F2` on a focused cell immediately switches the row to edit mode and *automatically focuses the input field* of that exact cell so you can start typing instantly.
- **Save Changes (`Enter`)**: While editing an input, pressing `Enter` saves the changes and restores keyboard focus back to the grid. (Note: `Enter` is disabled for textareas to allow multiline input).
- **Cancel Editing (`Escape`)**: Pressing `Escape` aborts the edit and gracefully returns focus to the grid cell you were previously highlighting.
- **Adjacent Cell Editing (`ArrowLeft` / `ArrowRight`)**: If you are typing inside an input and your text cursor reaches the very end (or very beginning) of the text, pressing the arrow key again will automatically jump focus to the next/previous cell's input in that row, allowing continuous horizontal data entry.

## 3. UI and Toolbar Optimizations
- **No Bulky Edit Buttons**: The explicit "Save" and "Cancel" buttons in the toolbar have been permanently removed to maximize screen real estate, as the keyboard workflow (`Enter` / `Escape`) entirely replaces them.
- **Responsive Search**: A dynamic global search input that collapses on mobile views.

## 4. Row Selection Behavior
- When navigating the grid with arrow keys, the internal row selection state (`selectionMode`) automatically follows your focused cell. 

---
*Note for AI/Future Developers: The focus management relies heavily on the `focusCell` method querying the DOM for `[data-row-index]` and `[data-col-index]` attributes, and resolving `<input>` tags inside them. Do not break these data attributes.*
