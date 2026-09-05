import { afterEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from './App'
import { validateAudioHeader } from './workers/audioValidation'
import type { AudioValidationResult } from './workers/audioValidation'

describe('App', () => {
  it('muestra el estado de carga mientras la petición está pendiente', async () => {
    globalThis.fetch = vi.fn(() => new Promise(() => {})) as unknown as typeof fetch

    render(<App />)

    const user = userEvent.setup()
    const file = new File(['audio'], 'audio.mp3', { type: 'audio/mpeg' })
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement

    await user.upload(fileInput, file)
    await user.click(screen.getByRole('button', { name: /subir/i }))

    expect(await screen.findByText(/subiendo archivo/i)).toBeInTheDocument()
  })

  describe('validación de audio con Web Worker', () => {
    const originalWorker = globalThis.Worker

    // jsdom no soporta Web Workers reales, así que se mockea globalThis.Worker
    // con una clase que reutiliza la misma validateAudioHeader que usaría el
    // Worker real, disparando onmessage en un microtask (como lo haría un Worker).
    class MockAudioValidatorWorker {
      onmessage: ((event: MessageEvent<AudioValidationResult>) => void) | null = null

      postMessage(message: { fileBuffer: ArrayBuffer; fileSize: number }) {
        const bytes = new Uint8Array(message.fileBuffer)
        const result = validateAudioHeader(bytes, message.fileSize)
        void Promise.resolve().then(() => {
          this.onmessage?.({ data: result } as MessageEvent<AudioValidationResult>)
        })
      }

      terminate() {}
    }

    afterEach(() => {
      globalThis.Worker = originalWorker
    })

    it('deshabilita el botón "Subir" y muestra el motivo cuando el header es inválido', async () => {
      globalThis.Worker = MockAudioValidatorWorker as unknown as typeof Worker

      render(<App />)

      const user = userEvent.setup()
      const invalidFile = new File(['no-es-un-header-de-audio'], 'fake.mp3', {
        type: 'audio/mpeg',
      })
      const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement

      await user.upload(fileInput, invalidFile)

      expect(await screen.findByText(/formato de audio no soportado/i)).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /subir/i })).toBeDisabled()
    })
  })
})
