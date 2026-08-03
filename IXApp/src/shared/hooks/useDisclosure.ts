import { useCallback, useState } from 'react';

export interface DisclosureControls {
  open: boolean;
  onOpen: () => void;
  onClose: () => void;
  onToggle: () => void;
  setOpen: (open: boolean) => void;
}

export const useDisclosure = (initialOpen = false): DisclosureControls => {
  const [open, setOpen] = useState(initialOpen);
  const onOpen = useCallback(() => setOpen(true), []);
  const onClose = useCallback(() => setOpen(false), []);
  const onToggle = useCallback(() => setOpen((current) => !current), []);

  return { open, onOpen, onClose, onToggle, setOpen };
};

