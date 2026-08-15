# Dialog FastTabs pattern

`FastTabsDialog.tsx` combines the shared dialog container with collapsible FastTabs; `index.ts` is its export boundary. Use it for modal forms whose fields naturally divide into sections. Do not use it for full-page editing or a small confirmation.

The caller owns open/close state, form state, validation, and save API behavior. The pattern owns modal/section composition only.

[Dialogs](../../shared/components/dialogs/README.md) · [FastTabs](../../shared/components/fast-tabs/README.md)
