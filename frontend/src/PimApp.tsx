import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import './PimApp.css'

type Product = {
  id: string
  sku: string
  name: string
  category: string
  status: string
  description: string
  price: number
  stock: number
  updatedAt: string
}

const apiBaseUrl = import.meta.env.VITE_API_URL ?? ''
const statusOptions = ['Published', 'Draft', 'Review']

function PimApp() {
  const [products, setProducts] = useState<Product[]>([])
  const [selectedId, setSelectedId] = useState('')
  const [form, setForm] = useState<Product | null>(null)
  const [search, setSearch] = useState('')
  const [category, setCategory] = useState('All categories')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    const loadProducts = async () => {
      try {
        const response = await fetch(`${apiBaseUrl}/api/products`)
        if (!response.ok) throw new Error('Unable to load the product catalog.')
        const loadedProducts: Product[] = await response.json()
        setProducts(loadedProducts)
        if (loadedProducts[0]) {
          setSelectedId(loadedProducts[0].id)
          setForm(loadedProducts[0])
        }
      } catch (loadError) {
        setError(loadError instanceof Error ? loadError.message : 'Unable to load products.')
      }
    }

    void loadProducts()
  }, [])

  const categories = useMemo(
    () => ['All categories', ...new Set(products.map((product) => product.category))],
    [products],
  )

  const filteredProducts = useMemo(() => {
    const query = search.trim().toLowerCase()
    return products.filter((product) => {
      const matchesSearch = !query || `${product.name} ${product.sku}`.toLowerCase().includes(query)
      const matchesCategory = category === 'All categories' || product.category === category
      return matchesSearch && matchesCategory
    })
  }, [category, products, search])

  const selectProduct = (product: Product) => {
    setSelectedId(product.id)
    setForm(product)
    setMessage('')
    setError('')
  }

  const updateField = <K extends keyof Product>(field: K, value: Product[K]) => {
    setForm((current) => (current ? { ...current, [field]: value } : current))
  }

  const saveProduct = async (event: FormEvent) => {
    event.preventDefault()
    if (!form) return

    setSaving(true)
    setMessage('')
    setError('')
    try {
      const response = await fetch(`${apiBaseUrl}/api/products/${form.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          sku: form.sku,
          name: form.name,
          category: form.category,
          status: form.status,
          description: form.description,
          price: Number(form.price),
          stock: Number(form.stock),
        }),
      })
      const body = await response.json()
      if (!response.ok) throw new Error(body.message ?? 'Unable to save product changes.')

      setProducts((current) => current.map((product) => (product.id === body.id ? body : product)))
      setForm(body)
      setMessage('Product changes saved and catalog refreshed.')
    } catch (saveError) {
      setError(saveError instanceof Error ? saveError.message : 'Unable to save product changes.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="pim-shell">
      <header className="topbar">
        <div className="brand-lockup">
          <div className="brand-mark">P</div>
          <div>
            <strong>Atlas PIM</strong>
            <span>Product information management</span>
          </div>
        </div>
        <div className="topbar-meta">
          <span className="environment">Staging</span>
          <span className="avatar">AM</span>
        </div>
      </header>

      <div className="workspace">
        <aside className="sidebar">
          <p className="sidebar-label">Workspace</p>
          <nav>
            <a className="nav-item active" href="#catalog">Catalog</a>
            <a className="nav-item" href="#imports">Imports</a>
            <a className="nav-item" href="#quality">Quality rules</a>
          </nav>
          <div className="sidebar-footer">
            <span className="online-dot" />
            All systems operational
          </div>
        </aside>

        <main className="content" id="catalog">
          <div className="content-heading">
            <div>
              <p className="breadcrumb">Workspace / Catalog</p>
              <h1>Product catalog</h1>
              <p className="subtitle">Manage product content, inventory, and publication status.</p>
            </div>
            <button className="primary-button" type="button" onClick={() => form && selectProduct(form)}>
              + New product
            </button>
          </div>

          <section className="catalog-layout">
            <div className="catalog-panel">
              <div className="catalog-toolbar">
                <label className="search-field">
                  <span>Search catalog</span>
                  <input
                    type="search"
                    placeholder="Search by product or SKU"
                    value={search}
                    onChange={(event) => setSearch(event.target.value)}
                  />
                </label>
                <label className="filter-field">
                  <span>Category</span>
                  <select value={category} onChange={(event) => setCategory(event.target.value)}>
                    {categories.map((option) => <option key={option}>{option}</option>)}
                  </select>
                </label>
              </div>
              <div className="catalog-summary">
                <strong>{filteredProducts.length} products</strong>
                <span>Last synced just now</span>
              </div>
              <div className="product-list">
                {filteredProducts.map((product) => (
                  <button
                    className={`product-row ${selectedId === product.id ? 'selected' : ''}`}
                    key={product.id}
                    type="button"
                    onClick={() => selectProduct(product)}
                  >
                    <span className="product-thumb">{product.name.slice(0, 1)}</span>
                    <span className="product-info">
                      <strong>{product.name}</strong>
                      <small>{product.sku} · {product.category}</small>
                    </span>
                    <span className={`status status-${product.status.toLowerCase()}`}>{product.status}</span>
                  </button>
                ))}
              </div>
            </div>

            <section className="editor-panel" aria-label="Product editor">
              {form ? (
                <form onSubmit={saveProduct}>
                  <div className="editor-heading">
                    <div>
                      <p className="section-kicker">Product details</p>
                      <h2>{form.name}</h2>
                    </div>
                    <span className={`status status-${form.status.toLowerCase()}`}>{form.status}</span>
                  </div>
                  <p className="editor-help">Edit the product content below. Changes are validated before publishing.</p>

                  <div className="form-grid">
                    <label className="form-field full-width">
                      <span>Product name</span>
                      <input value={form.name} onChange={(event) => updateField('name', event.target.value)} />
                    </label>
                    <label className="form-field">
                      <span>SKU <em>Required format: ABC-12345</em></span>
                      <input value={form.sku} onChange={(event) => updateField('sku', event.target.value.toUpperCase())} />
                    </label>
                    <label className="form-field">
                      <span>Category</span>
                      <input value={form.category} onChange={(event) => updateField('category', event.target.value)} />
                    </label>
                    <label className="form-field">
                      <span>Status</span>
                      <select value={form.status} onChange={(event) => updateField('status', event.target.value)}>
                        {statusOptions.map((option) => <option key={option}>{option}</option>)}
                      </select>
                    </label>
                    <label className="form-field">
                      <span>Price</span>
                      <input type="number" min="0" step="0.01" value={form.price} onChange={(event) => updateField('price', Number(event.target.value))} />
                    </label>
                    <label className="form-field">
                      <span>Stock on hand</span>
                      <input type="number" min="0" step="1" value={form.stock} onChange={(event) => updateField('stock', Number(event.target.value))} />
                    </label>
                    <label className="form-field full-width">
                      <span>Description</span>
                      <textarea rows={5} value={form.description} onChange={(event) => updateField('description', event.target.value)} />
                    </label>
                  </div>

                  <div className="editor-footer">
                    <small>Last updated {new Date(form.updatedAt).toLocaleString()}</small>
                    <button className="primary-button" disabled={saving} type="submit">
                      {saving ? 'Saving...' : 'Save changes'}
                    </button>
                  </div>
                  {message ? <p className="success-message">{message}</p> : null}
                  {error ? <p className="error-message">{error}</p> : null}
                </form>
              ) : (
                <p className="empty-editor">Select a product to begin editing.</p>
              )}
            </section>
          </section>
        </main>
      </div>
    </div>
  )
}

export default PimApp