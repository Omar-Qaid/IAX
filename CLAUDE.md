# IAX — Claude Code instructions

## Shell output discipline

- When running `sed` (or any command that prints many lines), redirect the output
  to a file in the scratchpad and show only the relevant lines (grep the file or
  print the specific line range). Never stream a full `sed` dump into the
  conversation.
