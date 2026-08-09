import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from './App'

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
})
