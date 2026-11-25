# Urban Wear - Clothing Store Manager (CS312)

Windows Forms (.NET 8) application for managing a clothing store.

## Windows Forms Controls & Event Handling
- Login with validation (`ErrorProvider`) and "Remember me"
- Main window with sidebar navigation between 6 UserControls
- Products: Add / Update / Delete, search + category filter, image picker, numeric-only price box
- Sizes & Colors: add/delete with duplicate checks, `ColorDialog`
- Inventory: filters, add/remove stock with validation, status colours
- Sales: add to order, cart, remove item, total, payment method, checkout receipt
- Reports: report type + date range, CSV export (`SaveFileDialog`)

## GDI+ / Graphics / Visualization
- Reports: bar chart + trend line drawn with `Graphics` in the `Paint` event
- Dashboard: KPI card accents + sparklines and a donut chart
- Login: gradient bar and T-shirt logo drawn with `GraphicsPath`
- Inventory: stock-level bars drawn inside grid cells (`CellPainting`)
- Sizes & Colors: owner-drawn colour list and glossy colour preview
- Products: placeholder image drawn with GDI+

Open `ClothingStore.sln` in Visual Studio 2022 and press F5.
Login: username `sara`, password `2005`.
