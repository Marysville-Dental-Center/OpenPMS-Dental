# How-to guides

Short, task-by-task guides for an office running Open PMS - Dental. Each guide gives the goal, the steps and a screenshot, and links to the
reference when you need more. The screenshots show fictional data only.

## Set up

| Guide | What you get |
|---|---|
| [Set up a new database](set-up-a-new-database.md) | A new, empty office database and the program connected to it |
| [Move from Open Dental](move-from-open-dental.md) | Your Open Dental database adopted, with its patients and history |
| [Installing in an office](../installing.md) (reference) | The database server settings, the workstations and updates |

## Imaging

| Guide | What you get |
|---|---|
| [Take X-rays with a sensor](take-x-rays.md) | A series of X-rays into a mount, slot by slot, in the capture session |
| [Set up the intraoral camera](set-up-the-intraoral-camera.md) | The camera's picture settings, and what each button on the handpiece does |
| [Take intraoral photos](take-intraoral-photos.md) | Photos from the intraoral camera into a photo mount or as quick photos |
| [Show images on a second monitor](show-images-on-a-second-monitor.md) | An image, mount, X-ray or the camera in a window of its own, on any monitor |

## Running the practice

| Guide | What you get |
|---|---|
| [Track inventory](track-inventory.md) | Stock that goes down as procedures are completed, with scanning, lots, counts and alerts |
| [Track routine maintenance](track-routine-maintenance.md) | A board of the office's maintenance routines, what is due, and a record of each time one is done |

## Writing a guide

Every new feature that staff or the administrator use gets a guide here, added to the tables above.

- **Concise:** one task per guide; a sentence on what it is for, then numbered steps; under a page. Details go in the reference, linked.
- **A screenshot** of the main window of the task, made by the opt-in screenshot tests with fictional data
  ([how screenshots are made](../screenshots/README.md)), saved in `docs/screenshots/` as `howto-<feature>.png`.
- **Names as the program shows them:** buttons and menus in **bold**, in the order the user clicks them.
- **Links that exist:** `python -I tools/check_doc_links.py` must pass.
