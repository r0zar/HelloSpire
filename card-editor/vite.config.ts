import { defineConfig } from "vite";

// UI on 5180, API on 2580. The proxy keeps every fetch same-origin so the
// server's CORS allowlist only ever sees the one dev origin.
export default defineConfig({
  server: {
    port: 5180,
    strictPort: true,
    proxy: { "/api": "http://127.0.0.1:2580" },
  },
});
