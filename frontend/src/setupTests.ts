import '@testing-library/jest-dom/vitest'
import { afterEach } from 'vitest'
import { cleanup } from '@testing-library/react'

afterEach(() => {
  cleanup()
})


class DefaultMockWorker {
  onmessage: ((event: MessageEvent) => void) | null = null

  constructor(_scriptURL: string | URL, _options?: WorkerOptions) {}

  postMessage(_message: unknown) {
    this.onmessage?.({ data: { valid: true } } as MessageEvent)
  }

  terminate() {}
}

if (typeof globalThis.Worker === 'undefined') {
  globalThis.Worker = DefaultMockWorker as unknown as typeof Worker
}
