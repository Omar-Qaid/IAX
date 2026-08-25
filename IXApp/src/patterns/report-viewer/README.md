# Report viewer

`ReportViewer` owns screen-preview behavior: toolbars, thumbnails, navigation, zoom,
search, full screen, printing, and export commands. `PrintoutDocument` owns the
physical page, standard company header/footer, page settings, and print CSS.

## Standard report

Render report-specific React content inside `PrintoutDocument`, then pass the document
to `ReportViewer`. Configure standard presentation through `headerConfig`,
`footerConfig`, `pageSettings`, and `viewerOptions`. Use the `header` or `footer` slots
only when a report must replace that standard section.

## Large server-backed report

Keep fetching in the report module using its normal API service and React Query. Pass a
controlled `pagination` object to the viewer:

```tsx
<ReportViewer
  pagination={{ currentPage, totalPages, loading, onPageChange, onPrefetchPage }}
  {...commands}
>
  <PrintoutDocument {...pageConfiguration}>{currentServerPage}</PrintoutDocument>
</ReportViewer>
```

Only `currentServerPage` is mounted. The thumbnail list is virtualized, so a large
logical page count does not create a large DOM. Backend export endpoints should be used
when an export must include pages that have not been loaded into the browser.

For printable tables, add the `printout-table` class to repeat table headers and the
`printout-keep-together` class to groups that should avoid page breaks.
