package main

import "github.com/rivo/tview"

func main() {
	app := tview.NewApplication()

	packageManagerApp := NewPackageManager(app, "https://api-v2v3search-0.nuget.org/query?q=%s&take=100&includeDelisted=false")
	packageManagerApp.Run()

	if err := app.Run(); err != nil {
		panic(err)
	}
}
