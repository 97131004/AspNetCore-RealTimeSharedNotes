import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'
import { readdirSync, statSync } from 'fs'

// auto-discover all .js entry points under src/ (skips .vue and .css)
function findEntries(dir) {
    const entries = {}
    const walk = (current) => {
        for (const file of readdirSync(current)) {
            const full = resolve(current, file)
            if (statSync(full).isDirectory()) {
                walk(full)
            } else if (file.endsWith('.js')) {
                entries[file.replace('.js', '')] = full
            }
        }
    }
    walk(dir)
    return entries
}

export default defineConfig({
    plugins: [vue()],
    resolve: {
        alias: { '@': resolve(__dirname, 'src') },
    },
    build: {
        outDir: '../wwwroot/dist',
        emptyOutDir: true,
        rollupOptions: {
            input: findEntries(resolve(__dirname, 'src')),
            output: {
                entryFileNames: 'js/[name].js',
                chunkFileNames: 'js/[name].js',
                assetFileNames: 'assets/[name][extname]',
            }
        }
    }
})
